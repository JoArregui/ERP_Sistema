using Microsoft.AspNetCore.Mvc;
using ERP.Services;
using System.Threading.Tasks;
using System;

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

        /// <summary>
        /// Registra la entrada validando el PIN del empleado
        /// </summary>
        [HttpPost("entrada")]
        public async Task<IActionResult> Entrada([FromBody] FichajeRequest request)
        {
            try 
            {
                var resultado = await _rrhhService.RegistrarEntradaConPin(request.EmpleadoId, request.Pin);
                
                if (resultado)
                {
                    return Ok(new { 
                        mensaje = "Entrada registrada correctamente",
                        hora = DateTime.Now.ToString("HH:mm:ss")
                    });
                }
                
                return Unauthorized("El PIN introducido es incorrecto.");
            }
            catch (Exception) // Se elimina 'ex' para evitar CS0168 si no se va a usar
            {
                return BadRequest("Ocurrió un error inesperado al procesar la entrada.");
            }
        }

        /// <summary>
        /// Registra la salida validando el PIN del empleado
        /// </summary>
        [HttpPost("salida")]
        public async Task<IActionResult> Salida([FromBody] FichajeRequest request)
        {
            try 
            {
                var resultado = await _rrhhService.RegistrarSalidaConPin(request.EmpleadoId, request.Pin);
                
                if (resultado)
                {
                    return Ok(new { 
                        mensaje = "Salida registrada correctamente",
                        hora = DateTime.Now.ToString("HH:mm:ss")
                    });
                }
                
                return Unauthorized("El PIN introducido es incorrecto.");
            }
            catch (Exception) // Se elimina 'ex' para evitar CS0168
            {
                return BadRequest("Ocurrió un error inesperado al procesar la salida.");
            }
        }
    }

    public class FichajeRequest
    {
        public required int EmpleadoId { get; set; }
        public required string Pin { get; set; } = string.Empty;
    }
}