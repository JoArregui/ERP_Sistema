using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Domain.Entities;
using ERP.Application.Services;
using ERP.Services;
using ERP.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FacturacionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly FacturacionService _facturacionService;
        private readonly PdfService _pdfService;

        public FacturacionController(
            ApplicationDbContext context, 
            FacturacionService facturacionService, 
            PdfService pdfService)
        {
            _context = context;
            _facturacionService = facturacionService;
            _pdfService = pdfService;
        }

        [HttpGet("listado")]
        public async Task<IActionResult> GetFacturas()
        {
            var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim)) return Unauthorized("Sesión inválida.");

            int empresaId = int.Parse(empresaIdClaim);

            var lista = await _context.Documentos
                .Include(d => d.Cliente)
                .Where(d => d.EmpresaId == empresaId && d.Tipo == TipoDocumento.Factura)
                .OrderByDescending(d => d.Fecha)
                .ToListAsync();

            return Ok(lista);
        }

        [HttpPost("guardar")] // Coincide con el llamado de tu Blazor
        public async Task<IActionResult> CrearFactura([FromBody] DocumentoComercial factura)
        {
            var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim)) return Unauthorized("Sesión inválida.");
            
            factura.EmpresaId = int.Parse(empresaIdClaim);

            // Delegamos toda la complejidad al servicio de aplicación
            // Esto gestiona: Guardado + Stock + MovimientoStock + Transaccionalidad
            var exito = await _facturacionService.RegistrarFacturaVentaAsync(factura);

            if (exito)
            {
                // Agregamos lógica de vencimiento rápida aquí o dentro del service
                return Ok(new { Message = "Factura procesada y stock actualizado", Id = factura.Id });
            }

            return BadRequest("No se pudo procesar la factura. Revise el stock de los productos.");
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
            
            byte[] pdfBytes = _pdfService.GenerarFacturaPdf(factura, empresa!, factura.Cliente!);
            
            return Ok(new 
            { 
                fileName = $"FACTURA_{factura.NumeroDocumento}.pdf", 
                content = Convert.ToBase64String(pdfBytes) 
            });
        }
    }
}