using ERP.Data;
using ERP.Domain.Entities;
using ERP.API.Services; 
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace ERP.Application.Services
{
    public class CicloFacturacionService
    {
        private readonly ApplicationDbContext _context;
        private readonly FacturacionService _facturacionService;
        private readonly StockService _stockService;

        public CicloFacturacionService(ApplicationDbContext context, 
                                      FacturacionService facturacionService, 
                                      StockService stockService)
        {
            _context = context;
            _facturacionService = facturacionService;
            _stockService = stockService;
        }

        public async Task<DocumentoComercial> ConvertirDocumento(int origenId, TipoDocumento nuevoTipo)
        {
            // 1. Iniciamos una transacción para asegurar que TODO ocurra o NADA ocurra
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var origen = await _context.Documentos
                    .Include(d => d.Lineas)
                    .FirstOrDefaultAsync(d => d.Id == origenId);

                if (origen == null) throw new Exception("El documento de origen no existe.");

                // Evitar duplicidades: ¿Ya se convirtió este documento al mismo tipo?
                var yaExiste = await _context.Documentos
                    .AnyAsync(d => d.DocumentoOrigenId == origenId && d.Tipo == nuevoTipo);
                
                if (yaExiste) 
                    throw new Exception($"Este documento ya ha sido convertido a {nuevoTipo} anteriormente.");

                var nuevoDoc = new DocumentoComercial
                {
                    Tipo = nuevoTipo,
                    EsCompra = origen.EsCompra,
                    EmpresaId = origen.EmpresaId,
                    ClienteId = origen.ClienteId,
                    ProveedorId = origen.ProveedorId,
                    DocumentoOrigenId = origen.Id,
                    MetodoPago = origen.MetodoPago,
                    UsuarioNombre = origen.UsuarioNombre,
                    Fecha = DateTime.Now,
                    BaseImponible = origen.BaseImponible,
                    TotalIva = origen.TotalIva,
                    Total = origen.Total,
                    Observaciones = $"Generado automáticamente desde {origen.Tipo} Nº {origen.NumeroDocumento}",
                    NumeroDocumento = await GenerarCorrelativo(nuevoTipo)
                };

                foreach (var linea in origen.Lineas)
                {
                    nuevoDoc.Lineas.Add(new DocumentoLinea
                    {
                        ArticuloId = linea.ArticuloId,
                        DescripcionArticulo = linea.DescripcionArticulo,
                        Cantidad = linea.Cantidad,
                        PrecioUnitario = linea.PrecioUnitario,
                        PorcentajeIva = linea.PorcentajeIva,
                        CategoriaNombre = linea.CategoriaNombre
                    });
                }

                // Lógica de Registro según flujo de negocio
                if (nuevoTipo == TipoDocumento.Factura)
                {
                    // La lógica de facturación suele ser compleja, delegamos pero dentro de la transacción
                    bool exito = await _facturacionService.RegistrarFacturaVentaAsync(nuevoDoc);
                    if (!exito) throw new Exception("El motor de facturación rechazó el documento.");
                }
                else
                {
                    _context.Documentos.Add(nuevoDoc);
                    await _context.SaveChangesAsync();

                    // Si es Albarán, afecta stock físicamente
                    if (nuevoTipo == TipoDocumento.Albaran)
                    {
                        await _stockService.ProcesarMovimientoStock(nuevoDoc.Id);
                    }
                }

                // 2. Confirmamos todos los cambios en la base de datos
                await transaction.CommitAsync();
                return nuevoDoc;
            }
            catch (Exception ex)
            {
                // 3. Si algo falla, se deshace cualquier cambio parcial
                await transaction.RollbackAsync();
                throw new Exception($"Error en ciclo de vida: {ex.Message}");
            }
        }

        public async Task<bool> IntentarEliminarAlbaran(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var albaran = await _context.Documentos.FindAsync(id);
                if (albaran == null) return false;

                // Validación de integridad: No eliminar si ya tiene hijos (Facturas)
                bool tieneVinculos = await _context.Documentos.AnyAsync(d => 
                    d.DocumentoOrigenId == id);

                if (tieneVinculos)
                    throw new Exception("Inconsistencia: No se puede eliminar un documento que ya ha generado otros registros en la cadena.");

                // Revertimos el stock antes de borrar el rastro
                await _stockService.LiberarReservaPorAnulacion(id);

                _context.Documentos.Remove(albaran);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<string> GenerarCorrelativo(TipoDocumento tipo)
        {
            // Diferenciador: Bloqueo de tabla para evitar números duplicados en concurrencia
            string prefijo = tipo switch
            {
                TipoDocumento.Factura => "FAC",
                TipoDocumento.Albaran => "ALB",
                TipoDocumento.Pedido => "PED",
                TipoDocumento.FacturaProforma => "PRO",
                _ => "DOC"
            };

            var anioActual = DateTime.Now.Year;
            
            // Buscamos el último número para ese tipo y año
            var ultimoDoc = await _context.Documentos
                .Where(d => d.Tipo == tipo && d.Fecha.Year == anioActual)
                .OrderByDescending(d => d.NumeroDocumento)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;

            if (ultimoDoc != null && !string.IsNullOrEmpty(ultimoDoc.NumeroDocumento))
            {
                // Extraemos la parte numérica final (ej: de FAC-2026-0005 extraemos 5)
                var partes = ultimoDoc.NumeroDocumento.Split('-');
                if (partes.Length > 0 && int.TryParse(partes.Last(), out int ultimoNumero))
                {
                    siguienteNumero = ultimoNumero + 1;
                }
            }

            return $"{prefijo}-{anioActual}-{siguienteNumero:D5}";
        }
    }
}