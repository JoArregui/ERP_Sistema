using Microsoft.AspNetCore.Mvc;
using ERP.Services;

namespace ERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FichajeController : ControllerBase
    {
        private readonly RRHHService _rrhhService;

        public FichajeController(RRHHService rrhhService)
        {
            _rrhhService = rrhhService;
        }

        [HttpPost("entrada/{empleadoId}")]
        public async Task<IActionResult> Entrada(int empleadoId)
        {
            await _rrhhService.RegistrarEntrada(empleadoId);
            return Ok($"Entrada registrada para el empleado {empleadoId} a las {DateTime.Now:HH:mm:ss}");
        }

        [HttpPost("salida/{empleadoId}")]
        public async Task<IActionResult> Salida(int empleadoId)
        {
            await _rrhhService.RegistrarSalida(empleadoId);
            return Ok($"Salida registrada para el empleado {empleadoId} a las {DateTime.Now:HH:mm:ss}");
        }
    }
}