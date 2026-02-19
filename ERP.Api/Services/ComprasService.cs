using Microsoft.EntityFrameworkCore;
using ERP.Data;
using ERP.Domain.Entities;
using ERP.Domain.DTOs;

namespace ERP.API.Services
{
    public class ComprasService
    {
        private readonly ApplicationDbContext _context;

        public ComprasService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Procesa la recepción de un pedido de compra.
        /// </summary>
        public async Task<bool> RecepcionarPedido(int pedidoId, string numeroAlbaran)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pedido = await _context.Documentos
                    .Include(d => d.Lineas)
                        .ThenInclude(l => l.Articulo)
                    .FirstOrDefaultAsync(d => d.Id == pedidoId && d.Tipo == TipoDocumento.Pedido);

                if (pedido == null || pedido.IsContabilizado)
                    return false;

                foreach (var linea in pedido.Lineas)
                {
                    var articulo = linea.Articulo;
                    if (articulo == null) continue;

                    // Recalcular PMP
                    decimal valorActual = articulo.Stock * articulo.PrecioCompra;
                    decimal valorNuevaEntrada = linea.Cantidad * linea.PrecioUnitario;
                    decimal nuevoStockTotal = articulo.Stock + linea.Cantidad;

                    if (nuevoStockTotal > 0)
                    {
                        articulo.PrecioCompra = (valorActual + valorNuevaEntrada) / nuevoStockTotal;
                    }

                    articulo.Stock += linea.Cantidad;

                    // Registro en Kardex
                    var movimiento = new MovimientoStock
                    {
                        ArticuloId = articulo.Id,
                        Fecha = DateTime.Now,
                        TipoMovimiento = "ENTRADA",
                        Cantidad = linea.Cantidad,
                        StockResultante = articulo.Stock,
                        ReferenciaDocumento = $"ALB: {numeroAlbaran}",
                        Observaciones = $"Recepción Pedido Compra Nº {pedido.NumeroDocumento}"
                    };

                    _context.MovimientosStock.Add(movimiento);
                }

                pedido.IsContabilizado = true;
                pedido.NumeroAlbaran = numeroAlbaran; 
                pedido.FechaRecepcion = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }

        /// <summary>
        /// Genera pedidos automáticos calculando el Total mediante la suma de Subtotales.
        /// </summary>
        public async Task<int> GenerarPedidoDesdeAlertas(List<AlertaStockDTO> alertas)
        {
            if (alertas == null || !alertas.Any()) return 0;

            int pedidosGenerados = 0;
            var alertasPorProveedor = alertas.GroupBy(a => a.ProveedorId);

            foreach (var grupo in alertasPorProveedor)
            {
                var proveedorId = grupo.Key;
                
                var nuevoPedido = new DocumentoComercial
                {
                    ProveedorId = proveedorId,
                    EmpresaId = 1, 
                    Fecha = DateTime.Now,
                    EsCompra = true,
                    Tipo = TipoDocumento.Pedido,
                    IsContabilizado = false,
                    NumeroDocumento = $"PAUTO-{DateTime.Now:yyyyMMdd-HHmm}",
                    Lineas = new List<DocumentoLinea>()
                };

                foreach (var item in grupo)
                {
                    var articulo = await _context.Articulos.FindAsync(item.ArticuloId);
                    if (articulo == null) continue;

                    var linea = new DocumentoLinea
                    {
                        ArticuloId = articulo.Id,
                        Cantidad = item.CantidadAReponer,
                        PrecioUnitario = articulo.PrecioCompra,
                        DescripcionArticulo = articulo.Descripcion,
                        PorcentajeIva = articulo.PorcentajeIva // Corregido: ya no requiere casteo a double
                    };

                    nuevoPedido.Lineas.Add(linea);
                }

                // Cálculo automático del Total basado en las líneas recién agregadas
                nuevoPedido.Total = nuevoPedido.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario);
                
                // Cálculo de Base e IVA
                nuevoPedido.BaseImponible = nuevoPedido.Total / 1.21m; 
                nuevoPedido.TotalIva = nuevoPedido.Total - nuevoPedido.BaseImponible;

                _context.Documentos.Add(nuevoPedido);
                pedidosGenerados++;
            }

            await _context.SaveChangesAsync();
            return pedidosGenerados;
        }
    }
}