using Microsoft.AspNetCore.Mvc;
using ERP.Domain.Entities;
using ERP.Services;
using ERP.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CicloFacturacionController : ControllerBase
    {
        private readonly CicloFacturacionService _cicloService;
        private readonly ApplicationDbContext _context;

        public CicloFacturacionController(CicloFacturacionService cicloService, ApplicationDbContext context)
        {
            _cicloService = cicloService;
            _context = context;
        }

        // 1. Obtener todos los documentos (Presupuestos, Pedidos, etc.)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentoComercial>>> GetDocumentos()
        {
            return await _context.Documentos
                .Include(d => d.Cliente)
                .Include(d => d.Lineas)
                .ToListAsync();
        }

        // 2. Obtener un documento por ID con sus líneas
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentoComercial>> GetDocumento(int id)
        {
            var doc = await _context.Documentos
                .Include(d => d.Lineas)
                .Include(d => d.Cliente)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return NotFound();
            return doc;
        }

        // 3. Crear un documento inicial (ej. un Presupuesto)
        [HttpPost]
        public async Task<ActionResult<DocumentoComercial>> CrearDocumento(DocumentoComercial nuevoDoc)
        {
            try
            {
                var resultado = await _cicloService.CrearDocumento(nuevoDoc);
                return CreatedAtAction(nameof(GetDocumento), new { id = resultado.Id }, resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 4. CONVERTIR: Pasar de un estado a otro (ej: Presupuesto -> Pedido)
        [HttpPost("{id}/convertir")]
        public async Task<ActionResult<DocumentoComercial>> Convertir(int id, [FromQuery] TipoDocumento nuevoTipo)
        {
            try
            {
                var documentoNuevo = await _cicloService.ConvertirDocumento(id, nuevoTipo);
                return Ok(documentoNuevo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}