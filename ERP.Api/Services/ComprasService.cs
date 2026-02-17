using ERP.Data;
using ERP.Domain.Entities;
using ERP.Domain.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Services
{
    public class ComprasService
    {
        private readonly ApplicationDbContext _context;
        private readonly StockService _stockService;

        public ComprasService(ApplicationDbContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        public async Task<int> GenerarPedidoDesdeAlertas(List<AlertaStockDTO> alertas)
        {
            if (alertas == null || !alertas.Any()) return 0;

            var alertasPorProveedor = alertas
                .Where(a => a.ArticuloId > 0)
                .GroupBy(a => a.ProveedorNombre);

            int pedidosGenerados = 0;
            var empresaDefault = await _context.Empresas.FirstOrDefaultAsync();
            if (empresaDefault == null) return 0;

            foreach (var grupo in alertasPorProveedor)
            {
                var primerArticuloId = grupo.First().ArticuloId;
                var articuloInfo = await _context.Articulos
                    .Include(a => a.ProveedorHabitual)
                    .FirstOrDefaultAsync(a => a.Id == primerArticuloId);

                if (articuloInfo?.ProveedorHabitual == null) continue;

                var nuevoPedido = new DocumentoComercial
                {
                    Tipo = TipoDocumento.Pedido,
                    EsCompra = true,
                    Fecha = DateTime.Now,
                    NumeroDocumento = $"PED-{DateTime.Now:yyyyMMddHHmm}-{articuloInfo.ProveedorHabitual.Id}",
                    ProveedorId = articuloInfo.ProveedorHabitualId,
                    EmpresaId = empresaDefault.Id,
                    MetodoPago = "Transferencia",
                    IsContabilizado = false,
                    Lineas = new List<DocumentoLinea>()
                };

                decimal totalBase = 0;
                decimal totalIva = 0;

                foreach (var alerta in grupo)
                {
                    var art = await _context.Articulos.FindAsync(alerta.ArticuloId);
                    if (art == null) continue;

                    var linea = new DocumentoLinea
                    {
                        ArticuloId = art.Id,
                        DescripcionArticulo = art.Descripcion,
                        Cantidad = alerta.CantidadSugerida,
                        PrecioUnitario = art.PrecioCompra,
                        PorcentajeIva = (double)art.PorcentajeIva 
                    };

                    decimal subtotalLinea = linea.Cantidad * linea.PrecioUnitario;
                    totalBase += subtotalLinea;
                    totalIva += subtotalLinea * (decimal)(linea.PorcentajeIva / 100);

                    nuevoPedido.Lineas.Add(linea);
                }

                nuevoPedido.BaseImponible = totalBase;
                nuevoPedido.TotalIva = totalIva;
                nuevoPedido.Total = totalBase + totalIva;

                _context.Documentos.Add(nuevoPedido);
                pedidosGenerados++;
            }

            await _context.SaveChangesAsync();
            return pedidosGenerados;
        }

        public async Task<bool> RecepcionarPedido(int pedidoId, string numeroAlbaranProveedor)
        {
            var pedido = await _context.Documentos
                .Include(d => d.Lineas)
                .FirstOrDefaultAsync(d => d.Id == pedidoId && d.EsCompra && !d.IsContabilizado);

            if (pedido == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                pedido.Tipo = TipoDocumento.Factura; 
                pedido.NumeroFacturaProveedor = numeroAlbaranProveedor;
                pedido.IsContabilizado = true;

                await _context.SaveChangesAsync();

                // Aquí es donde ocurre la magia del stock
                await _stockService.ProcesarMovimientoStock(pedido.Id);

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}