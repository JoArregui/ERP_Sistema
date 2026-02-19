using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Domain.Entities;
using ERP.Data;
using ERP.Services;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CierreCajaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;

        public CierreCajaController(ApplicationDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpGet("totales-pendientes/{empresaId}")]
        public async Task<ActionResult<CierreCaja>> GetTotalesPendientes(int empresaId)
        {
            try
            {
                // Obtenemos facturas de venta no contabilizadas con sus líneas
                var docs = await _context.Documentos
                    .Include(d => d.Lineas)
                    .Where(d => d.EmpresaId == empresaId && !d.IsContabilizado && !d.EsCompra && d.Tipo == TipoDocumento.Factura)
                    .ToListAsync();

                var lineas = docs.SelectMany(d => d.Lineas).ToList();

                var cierre = new CierreCaja
                {
                    EmpresaId = empresaId,
                    FechaCierre = DateTime.Now,
                    Terminal = "TPV-01",
                    TotalVentasEfectivo = docs.Where(d => d.MetodoPago == "Efectivo").Sum(d => d.Total),
                    TotalVentasTarjeta = docs.Where(d => d.MetodoPago == "Tarjeta").Sum(d => d.Total),
                    
                    // Cálculo de desgloses fiscales
                    Base21 = lineas.Where(l => l.PorcentajeIva == 21).Sum(l => l.Cantidad * l.PrecioUnitario),
                    Iva21 = lineas.Where(l => l.PorcentajeIva == 21).Sum(l => (l.Cantidad * l.PrecioUnitario) * 0.21m),
                    
                    Base10 = lineas.Where(l => l.PorcentajeIva == 10).Sum(l => l.Cantidad * l.PrecioUnitario),
                    Iva10 = lineas.Where(l => l.PorcentajeIva == 10).Sum(l => (l.Cantidad * l.PrecioUnitario) * 0.10m),

                    Base4 = lineas.Where(l => l.PorcentajeIva == 4).Sum(l => l.Cantidad * l.PrecioUnitario),
                    Iva4 = lineas.Where(l => l.PorcentajeIva == 4).Sum(l => (l.Cantidad * l.PrecioUnitario) * 0.04m),

                    TotalIva = docs.Sum(d => d.TotalIva),
                    Observaciones = "" 
                };

                return Ok(cierre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener datos: {ex.Message}");
            }
        }

        [HttpPost("ejecutar")]
        public async Task<IActionResult> EjecutarCierre([FromBody] CierreCaja cierre)
        {
            if (cierre == null) return BadRequest("Datos de cierre inválidos.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                cierre.IsProcesado = true;
                cierre.FechaCierre = DateTime.Now;
                _context.CierresCaja.Add(cierre);

                // Marcamos los documentos como contabilizados para que no salgan en el próximo cierre
                var documentos = await _context.Documentos
                    .Where(d => d.EmpresaId == cierre.EmpresaId && !d.IsContabilizado && !d.EsCompra && d.Tipo == TipoDocumento.Factura)
                    .ToListAsync();

                foreach (var doc in documentos)
                {
                    doc.IsContabilizado = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { id = cierre.Id, message = "Cierre de caja completado con éxito" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error crítico: {ex.Message}");
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DescargarCierrePdf(int id)
        {
            var cierre = await _context.CierresCaja
                .Include(c => c.Empresa)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cierre == null || cierre.Empresa == null) return NotFound();

            var pdfBytes = _pdfService.GenerarCierreCajaPdf(cierre, cierre.Empresa);
            return File(pdfBytes, "application/pdf", $"Cierre_{cierre.FechaCierre:yyyyMMdd}.pdf");
        }
    }
}