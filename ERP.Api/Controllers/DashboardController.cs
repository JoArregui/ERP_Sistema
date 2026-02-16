using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Data;
using ERP.Api.Models;
using ERP.Domain.Entities;

namespace ERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("resumen-financiero")]
        public async Task<ActionResult<DashboardDTO>> GetResumen()
        {
            // 1. Ventas totales (Facturas de venta)
            var ventas = await _context.Documentos
                .Where(d => d.Tipo == TipoDocumento.Factura && !d.EsCompra)
                .SumAsync(d => d.Total);

            // 2. Compras totales (Facturas de compra)
            var compras = await _context.Documentos
                .Where(d => d.Tipo == TipoDocumento.Factura && d.EsCompra)
                .SumAsync(d => d.Total);

            // 3. Gastos de personal (Nóminas generadas)
            var nominas = await _context.Nominas
                .SumAsync(n => n.SalarioBase + n.Complementos);

            // 4. Datos de Tesorería (Lo que nos deben)
            // CORRECCIÓN: Usamos Estado != "Pagado"
            var pendientes = await _context.Vencimientos
                .Where(v => v.Estado != "Pagado" && v.Documento != null && !v.Documento.EsCompra)
                .ToListAsync();

            // 5. Stock Crítico
            var stockCritico = await _context.Articulos
                .CountAsync(a => a.Stock < 5);

            var dashboard = new DashboardDTO
            {
                TotalVentas = ventas,
                TotalCompras = compras,
                TotalNominas = nominas,
                FacturasPendientesCobro = pendientes.Count,
                ImportePendienteCobro = pendientes.Sum(p => p.Importe),
                ArticulosStockBajo = stockCritico
            };

            return Ok(dashboard);
        }
    }
}