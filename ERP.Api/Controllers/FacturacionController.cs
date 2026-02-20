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

        /// <summary>
        /// Cambiado de "guardar" a "crear-factura" para coincidir con NuevaVenta.razor
        /// </summary>
        [HttpPost("crear-factura")]
        public async Task<IActionResult> CrearFactura([FromBody] DocumentoComercial factura)
        {
            var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim)) return Unauthorized("Sesión inválida.");
            
            factura.EmpresaId = int.Parse(empresaIdClaim);

            // El servicio gestiona: Guardado + Stock + MovimientoStock + Transaccionalidad
            var exito = await _facturacionService.RegistrarFacturaVentaAsync(factura);

            if (exito)
            {
                // Devolvemos el ID para que el frontend pueda generar el PDF/Ticket inmediatamente
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
            
            // Generación de PDF A4 estándar
            byte[] pdfBytes = _pdfService.GenerarFacturaPdf(factura, empresa!, factura.Cliente!);
            
            // Retornamos el archivo directamente como stream para mayor eficiencia
            return File(pdfBytes, "application/pdf", $"FACTURA_{factura.NumeroDocumento}.pdf");
        }

        /// <summary>
        /// Endpoint para la impresión térmica de tickets (formato 80mm)
        /// </summary>
        [HttpGet("descargar-ticket/{id}")]
        public async Task<IActionResult> DescargarTicket(int id)
        {
            var factura = await _context.Documentos
                .Include(d => d.Lineas)
                .Include(d => d.Cliente)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (factura == null) return NotFound();

            var empresa = await _context.Empresas.FindAsync(factura.EmpresaId);

            // Aquí llamamos a un método específico del PdfService para formato Ticket
            // Si no lo tienes, el PdfService debería tener una variante para tamaños pequeños
            byte[] pdfBytes = _pdfService.GenerarTicketPdf(factura, empresa!, factura.Cliente!);

            return File(pdfBytes, "application/pdf", $"TICKET_{factura.NumeroDocumento}.pdf");
        }
    }
}