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
            var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim)) return Unauthorized("Sesión inválida.");
            
            int empresaId = int.Parse(empresaIdClaim);
            factura.EmpresaId = empresaId;
            factura.Fecha = DateTime.Now;

            // --- CÁLCULO PROFESIONAL DE TOTALES ---
            factura.BaseImponible = factura.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario);
            factura.TotalIva = factura.Lineas.Sum(l => (l.Cantidad * l.PrecioUnitario) * (decimal)(l.PorcentajeIva / 100));
            factura.Total = factura.BaseImponible + factura.TotalIva;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. GESTIÓN DE STOCK (Antes de guardar la factura)
                foreach (var linea in factura.Lineas)
                {
                    var articulo = await _context.Articulos.FindAsync(linea.ArticuloId);
                    if (articulo == null) throw new Exception($"El artículo con ID {linea.ArticuloId} no existe.");
                    
                    if (articulo.Stock < linea.Cantidad)
                    {
                        // Opcional: Podrías permitir stock negativo si tu negocio lo requiere
                        // throw new Exception($"Stock insuficiente para: {articulo.Descripcion}");
                    }

                    articulo.Stock -= linea.Cantidad;
                    _context.Articulos.Update(articulo);
                }

                // 2. PERSISTENCIA DEL DOCUMENTO
                _context.Documentos.Add(factura);
                await _context.SaveChangesAsync();

                // 3. GESTIÓN DE VENCIMIENTOS (Tesorería)
                var cliente = await _context.Clientes.FindAsync(factura.ClienteId);
                var fechaVencimiento = DateTime.Now.AddDays(30);
                
                if (cliente != null && cliente.DiaPagoHabitual > 0)
                {
                    fechaVencimiento = new DateTime(fechaVencimiento.Year, fechaVencimiento.Month, cliente.DiaPagoHabitual);
                    if (fechaVencimiento < DateTime.Now) fechaVencimiento = fechaVencimiento.AddMonths(1);
                }

                var vencimiento = new Vencimiento
                {
                    DocumentoId = factura.Id,
                    FechaVencimiento = fechaVencimiento,
                    Importe = factura.Total,
                    Estado = "Pendiente",
                    EmpresaId = empresaId
                };
                
                _context.Vencimientos.Add(vencimiento);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { Message = "Operación exitosa", Id = factura.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error crítico: {ex.Message}");
            }
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
            
            // Generación del PDF
            byte[] pdfBytes = _pdfService.GenerarFacturaPdf(factura, empresa!, factura.Cliente!);
            
            return File(pdfBytes, "application/pdf", $"{factura.Tipo}_{factura.NumeroDocumento}.pdf");
        }
    }
}