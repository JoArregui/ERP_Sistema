using ERP.Data;
using ERP.Domain.Entities;
using ERP.API.Services; // Donde reside StockService
using Microsoft.EntityFrameworkCore;

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
            var origen = await _context.Documentos
                .Include(d => d.Lineas)
                .FirstOrDefaultAsync(d => d.Id == origenId);

            if (origen == null) throw new Exception("Origen no encontrado.");

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
                Observaciones = $"Generado desde {origen.Tipo} Nº {origen.NumeroDocumento}",
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

            if (nuevoTipo == TipoDocumento.Factura)
            {
                bool exito = await _facturacionService.RegistrarFacturaVentaAsync(nuevoDoc);
                if (!exito) throw new Exception("Error al registrar la factura.");
            }
            else if (nuevoTipo == TipoDocumento.Albaran)
            {
                _context.Documentos.Add(nuevoDoc);
                await _context.SaveChangesAsync();
                await _stockService.ProcesarMovimientoStock(nuevoDoc.Id);
            }
            else
            {
                _context.Documentos.Add(nuevoDoc);
                await _context.SaveChangesAsync();
            }

            return nuevoDoc;
        }

        // MÉTODO QUE EL CONTROLADOR NO ENCONTRABA
        public async Task<bool> IntentarEliminarAlbaran(int id)
        {
            var albaran = await _context.Documentos.FindAsync(id);
            if (albaran == null) return false;

            bool facturado = await _context.Documentos.AnyAsync(d => 
                d.DocumentoOrigenId == id && d.Tipo == TipoDocumento.Factura);

            if (facturado)
                throw new Exception("Acción denegada: Este albarán ya está vinculado a una factura.");

            await _stockService.LiberarReservaPorAnulacion(id);

            _context.Documentos.Remove(albaran);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<string> GenerarCorrelativo(TipoDocumento tipo)
        {
            string prefijo = tipo switch
            {
                TipoDocumento.Factura => "FAC-",
                TipoDocumento.Albaran => "ALB-",
                TipoDocumento.Pedido => "PED-",
                TipoDocumento.FacturaProforma => "PRO-",
                _ => "DOC-"
            };
            int count = await _context.Documentos.CountAsync(d => d.Tipo == tipo);
            return $"{prefijo}{DateTime.Now.Year}-{(count + 1):D4}";
        }
    }
}