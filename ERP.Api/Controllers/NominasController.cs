using Microsoft.AspNetCore.Mvc;
using ERP.Services;
using ERP.Domain.Entities;

namespace ERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NominasController : ControllerBase
    {
        private readonly NominaService _nominaService;

        public NominasController(NominaService nominaService)
        {
            _nominaService = nominaService;
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
    }
}