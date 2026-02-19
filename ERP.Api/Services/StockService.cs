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
        /// Procesa un documento completo, actualiza stocks, recalcula precios medios 
        /// e incrementa la trazabilidad mediante MovimientoStock.
        /// </summary>
        public async Task ProcesarMovimientoStock(int documentoId)
        {
            // Mantenemos la transaccionalidad atómica para asegurar la integridad Stock <-> Documento
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

                    string tipoMov;
                    decimal cantidadMov = linea.Cantidad;

                    if (doc.EsCompra) 
                    {
                        tipoMov = "ENTRADA";
                        
                        // --- INTELIGENCIA DE NEGOCIO: PRECIO MEDIO PONDERADO (PMP) ---
                        decimal stockPrevio = articulo.Stock;
                        decimal nuevoStock = articulo.Stock + linea.Cantidad;

                        // Solo recalculamos si el nuevo stock es positivo para evitar errores matemáticos
                        if (nuevoStock > 0)
                        {
                            // Fórmula: ((Stock Actual * Precio Actual) + (Cantidad Nueva * Precio Nuevo)) / Stock Total
                            articulo.PrecioCompra = ((stockPrevio * articulo.PrecioCompra) + (linea.Cantidad * linea.PrecioUnitario)) / nuevoStock;
                        }

                        articulo.Stock = nuevoStock;
                    }
                    else 
                    {
                        tipoMov = "SALIDA";
                        
                        // Actualizar Stock Físico por Venta
                        articulo.Stock -= linea.Cantidad;
                    }

                    // --- AUDITORÍA Y TRAZABILIDAD ---
                    var movimiento = new MovimientoStock
                    {
                        Fecha = DateTime.Now,
                        ArticuloId = articulo.Id,
                        EmpresaId = doc.EmpresaId, // Importante para filtros multi-empresa
                        TipoMovimiento = tipoMov,
                        Cantidad = cantidadMov,
                        StockResultante = articulo.Stock,
                        ReferenciaDocumento = doc.NumeroDocumento,
                        Observaciones = $"Procesado automáticamente desde {doc.Tipo} (ID: {doc.Id})"
                    };

                    _context.MovimientosStock.Add(movimiento);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Relanzamos la excepción para que el FacturacionService sepa que el proceso falló
                throw new Exception($"Fallo crítico en el proceso de stock: {ex.Message}", ex);
            }
        }
    }
}