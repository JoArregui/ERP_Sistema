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
        /// Procesa un documento completo: gestiona Reservas (Albarán) o Salidas Físicas (Factura).
        /// </summary>
        public async Task ProcesarMovimientoStock(int documentoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var doc = await _context.Documentos
                    .Include(d => d.Lineas)
                    .FirstOrDefaultAsync(d => d.Id == documentoId);

                if (doc == null || doc.Lineas == null) return;

                foreach (var linea in doc.Lineas)
                {
                    var articulo = await _context.Articulos.FindAsync(linea.ArticuloId);
                    if (articulo == null) continue;

                    string tipoMov = "";
                    decimal cantidadMov = linea.Cantidad;

                    // --- LÓGICA DE COMPRAS (ENTRADAS) ---
                    if (doc.EsCompra) 
                    {
                        tipoMov = "ENTRADA";
                        decimal stockPrevio = articulo.Stock;
                        articulo.Stock += linea.Cantidad;

                        // Recálculo del Precio Medio Ponderado (PMP)
                        if (articulo.Stock > 0)
                        {
                            articulo.PrecioCompra = ((stockPrevio * articulo.PrecioCompra) + (linea.Cantidad * linea.PrecioUnitario)) / articulo.Stock;
                        }
                    }
                    // --- LÓGICA DE VENTAS (ALBARÁN = RESERVA / FACTURA = SALIDA) ---
                    else 
                    {
                        if (doc.Tipo == TipoDocumento.Albaran)
                        {
                            tipoMov = "RESERVA_BLOQUEO";
                            articulo.StockReservado += linea.Cantidad; // Incrementa el bloqueo
                        }
                        else if (doc.Tipo == TipoDocumento.Factura)
                        {
                            tipoMov = "SALIDA_VENTA";
                            
                            // Si la factura nace de un Albarán previo, liberamos la reserva primero
                            if (doc.DocumentoOrigenId.HasValue)
                            {
                                var docOrigen = await _context.Documentos.AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == doc.DocumentoOrigenId);
                                
                                if (docOrigen != null && docOrigen.Tipo == TipoDocumento.Albaran)
                                {
                                    articulo.StockReservado -= linea.Cantidad; // Libera el bloqueo
                                }
                            }
                            
                            articulo.Stock -= linea.Cantidad; // Resta del físico real
                        }
                    }

                    // REGISTRO DE MOVIMIENTO PARA AUDITORÍA
                    _context.MovimientosStock.Add(new MovimientoStock
                    {
                        Fecha = DateTime.Now,
                        ArticuloId = articulo.Id,
                        EmpresaId = doc.EmpresaId,
                        TipoMovimiento = tipoMov,
                        Cantidad = cantidadMov,
                        StockResultante = articulo.Stock,
                        ReferenciaDocumento = doc.NumeroDocumento,
                        Observaciones = $"Procesado desde {doc.Tipo} (ID: {doc.Id})."
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error crítico en stock: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Libera la reserva si un albarán es eliminado antes de ser facturado.
        /// </summary>
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
                    articulo.StockReservado -= linea.Cantidad; // Devuelve el stock a 'Disponible'
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}