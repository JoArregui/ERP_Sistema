using Microsoft.AspNetCore.Mvc;
using ERP.Services;
using ERP.Domain.Entities;
using ERP.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NominasController : ControllerBase
    {
        private readonly NominaService _nominaService;
        private readonly ApplicationDbContext _context;

        public NominasController(NominaService nominaService, ApplicationDbContext context)
        {
            _nominaService = nominaService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Nomina>>> GetNominas([FromQuery] int mes, [FromQuery] int anio)
        {
            return await _context.Nominas
                .Include(n => n.Empleado)
                .Where(n => n.Mes == mes && n.Anio == anio)
                .ToListAsync();
        }

        [HttpPost("generar/{empleadoId}")]
        public async Task<IActionResult> Generar(int empleadoId, [FromQuery] int mes, [FromQuery] int anio)
        {
            try 
            {
                var nomina = await _nominaService.GenerarNominaMensual(empleadoId, mes, anio);
                return Ok(nomina);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("pagar/{id}")]
        public async Task<IActionResult> Pagar(int id)
        {
            try 
            {
                var resultado = await _nominaService.ProcesarPagoNominaAsync(id);
                if (resultado) return Ok(new { message = "Pago procesado y Tesorería actualizada exitosamente." });
                return NotFound("La nómina no existe, el empleado no está vinculado o ya ha sido pagada.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}