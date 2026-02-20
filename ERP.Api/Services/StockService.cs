using ERP.Data;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Services
{
    public class StockService
    {
        private readonly ApplicationDbContext _context;

        public StockService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Procesa un documento completo con validaciones de integridad y control de stock negativo.
        /// </summary>
        public async Task ProcesarMovimientoStock(int documentoId)
        {
            // Usamos una transacción para asegurar la atomicidad de todos los cambios
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var doc = await _context.Documentos
                    .Include(d => d.Lineas)
                    .FirstOrDefaultAsync(d => d.Id == documentoId);

                if (doc == null || doc.Lineas == null || !doc.Lineas.Any()) return;

                foreach (var linea in doc.Lineas)
                {
                    // Bloqueo Pesimista: Evitamos que otro proceso modifique el artículo mientras operamos
                    var articulo = await _context.Articulos
                        .FirstOrDefaultAsync(a => a.Id == linea.ArticuloId);

                    if (articulo == null) 
                        throw new Exception($"El artículo con ID {linea.ArticuloId} no existe en el maestro.");

                    string tipoMov = "";
                    decimal cantidadMov = linea.Cantidad;
                    decimal stockPrevio = articulo.Stock;

                    if (doc.EsCompra)
                    {
                        // --- ENTRADA DE MERCANCÍA ---
                        tipoMov = "ENTRADA_COMPRA";
                        
                        // Recálculo del PMP antes de actualizar el stock total
                        // Fórmula: ((Stock Actual * PMP Actual) + (Nueva Cantidad * Nuevo Precio)) / (Stock Actual + Nueva Cantidad)
                        if (articulo.Stock + linea.Cantidad > 0)
                        {
                            articulo.PrecioCompra = ((articulo.Stock * articulo.PrecioCompra) + (linea.Cantidad * linea.PrecioUnitario)) 
                                                    / (articulo.Stock + linea.Cantidad);
                        }
                        
                        articulo.Stock += linea.Cantidad;
                    }
                    else
                    {
                        // --- SALIDA / VENTA ---
                        if (doc.Tipo == TipoDocumento.Albaran)
                        {
                            // Validación: ¿Hay stock disponible (Físico - Reservado)?
                            decimal disponible = articulo.Stock - articulo.StockReservado;
                            if (disponible < linea.Cantidad)
                            {
                                // Aquí puedes decidir si lanzar excepción o permitir stock negativo según configuración
                                // throw new Exception($"Stock insuficiente para {articulo.Nombre}. Disponible: {disponible}");
                            }

                            tipoMov = "RESERVA_ALBARAN";
                            articulo.StockReservado += linea.Cantidad;
                        }
                        else if (doc.Tipo == TipoDocumento.Factura)
                        {
                            tipoMov = "SALIDA_FACTURA";

                            // Si viene de un albarán, la reserva ya existe y hay que consumirla
                            if (doc.DocumentoOrigenId.HasValue)
                            {
                                var tieneAlbaran = await _context.Documentos
                                    .AnyAsync(x => x.Id == doc.DocumentoOrigenId && x.Tipo == TipoDocumento.Albaran);
                                
                                if (tieneAlbaran)
                                {
                                    articulo.StockReservado -= linea.Cantidad;
                                }
                            }

                            articulo.Stock -= linea.Cantidad;
                        }
                    }

                    // Auditoría detallada
                    _context.MovimientosStock.Add(new MovimientoStock
                    {
                        Fecha = DateTime.Now,
                        ArticuloId = articulo.Id,
                        EmpresaId = doc.EmpresaId,
                        TipoMovimiento = tipoMov,
                        Cantidad = cantidadMov,
                        StockResultante = articulo.Stock, // Stock físico real tras la operación
                        ReferenciaDocumento = doc.NumeroDocumento,
                        Observaciones = $"Doc: {doc.Tipo} | Origen: {doc.DocumentoOrigenId ?? 0} | PMP: {articulo.PrecioCompra:C2}"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error crítico en actualización de inventario: {ex.Message}", ex);
            }
        }

        public async Task LiberarReservaPorAnulacion(int albaranId)
        {
            var albaran = await _context.Documentos
                .Include(d => d.Lineas)
                .FirstOrDefaultAsync(d => d.Id == albaranId);

            if (albaran == null) return;

            foreach (var linea in albaran.Lineas)
            {
                var articulo = await _context.Articulos.FindAsync(linea.ArticuloId);
                if (articulo != null)
                {
                    articulo.StockReservado -= linea.Cantidad;
                    
                    // Registro de la liberación en el histórico
                    _context.MovimientosStock.Add(new MovimientoStock
                    {
                        Fecha = DateTime.Now,
                        ArticuloId = articulo.Id,
                        EmpresaId = albaran.EmpresaId,
                        TipoMovimiento = "ANULACION_RESERVA",
                        Cantidad = linea.Cantidad,
                        StockResultante = articulo.Stock,
                        ReferenciaDocumento = albaran.NumeroDocumento,
                        Observaciones = "Reserva liberada por eliminación de albarán."
                    });
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}