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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentoComercial>>> GetDocumentos()
        {
            return await _context.Documentos
                .Include(d => d.Cliente)
                .Include(d => d.Lineas)
                .ToListAsync();
        }

        [HttpPost("{id}/convertir")]
        public async Task<ActionResult<DocumentoComercial>> Convertir(int id, [FromQuery] TipoDocumento nuevoTipo)
        {
            try
            {
                // Al convertir a FACTURA, el CicloFacturacionService debería llamar internamente 
                // al StockService para que el flujo sea automático.
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