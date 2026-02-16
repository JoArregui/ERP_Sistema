using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Domain.Entities;
using ERP.Services;
using ERP.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERP.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FacturacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;

        public FacturacionController(ApplicationDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpPost("crear-factura")]
        public async Task<IActionResult> CrearFactura([FromBody] DocumentoComercial factura)
        {
            // Extraer el EmpresaId del Token del usuario (Multi-tenant)
            var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim)) return Unauthorized("No se encontró la empresa del usuario.");
            
            int empresaId = int.Parse(empresaIdClaim);
            factura.EmpresaId = empresaId;
            factura.Fecha = DateTime.Now;

            // 1. Calcular Totales de forma robusta
            factura.BaseImponible = factura.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario);
            factura.TotalIva = factura.BaseImponible * 0.21m; // Asumimos 21% para este ejemplo
            factura.Total = factura.BaseImponible + factura.TotalIva;

            // 2. Guardar en BD
            _context.Documentos.Add(factura);
            await _context.SaveChangesAsync();

            // 3. Crear Vencimiento automático a 30 días
            var vencimiento = new Vencimiento
            {
                DocumentoId = factura.Id,
                FechaVencimiento = DateTime.Now.AddDays(30),
                Importe = factura.Total,
                Estado = "Pendiente",
                EmpresaId = empresaId
            };
            _context.Vencimientos.Add(vencimiento);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Factura creada y vencimiento programado.", FacturaId = factura.Id });
        }

        [HttpGet("descargar-pdf/{id}")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var factura = await _context.Documentos
                .Include(d => d.Lineas)
                .Include(d => d.Cliente)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (factura == null) return NotFound();

            var empresa = await _context.Empresas.FindAsync(factura.EmpresaId);
            
            byte[] pdfBytes = _pdfService.GenerarFacturaPdf(factura, empresa!, factura.Cliente);
            
            return File(pdfBytes, "application/pdf", $"Factura_{factura.NumeroDocumento}.pdf");
        }
    }
}