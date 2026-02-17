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

        public async Task ProcesarMovimientoStock(int documentoId)
        {
            // Cargamos el documento con sus líneas
            var doc = await _context.Documentos
                .Include(d => d.Lineas)
                .FirstOrDefaultAsync(d => d.Id == documentoId);

            if (doc == null) return;

            foreach (var linea in doc.Lineas)
            {
                var articulo = await _context.Articulos.FindAsync(linea.ArticuloId);
                if (articulo == null) continue;

                if (doc.EsCompra) 
                {
                    // --- FLUJO DE ENTRADA (Proveedor -> Almacén) ---
                    
                    // 1. Recalcular Precio Medio de Compra antes de sumar stock
                    // Formula: ((StockActual * PrecioCompraActual) + (NuevaCantidad * PrecioNuevaCompra)) / (StockTotal)
                    decimal stockPrevio = articulo.Stock;
                    decimal nuevoStock = articulo.Stock + linea.Cantidad;

                    if (nuevoStock > 0)
                    {
                        articulo.PrecioCompra = ((stockPrevio * articulo.PrecioCompra) + (linea.Cantidad * linea.PrecioUnitario)) / nuevoStock;
                    }

                    // 2. Aumentar Stock
                    articulo.Stock += linea.Cantidad;
                }
                else 
                {
                    // --- FLUJO DE SALIDA (Almacén -> Cliente/TPV) ---
                    articulo.Stock -= linea.Cantidad;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}