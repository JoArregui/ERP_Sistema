using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Domain.Entities;
using ERP.Data;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CierreCajaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CierreCajaController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Recupera el resumen de lo que el sistema espera encontrar en caja.
        /// </summary>
        [HttpGet("totales-pendientes")]
        public async Task<ActionResult<CierreCaja>> GetTotalesPendientes()
        {
            try
            {
                // Filtramos ventas que no estén en un cierre previo
                var docs = await _context.Documentos
                    .Where(d => !d.IsContabilizado && !d.EsCompra)
                    .ToListAsync();

                var cierre = new CierreCaja
                {
                    FechaCierre = DateTime.Now,
                    Terminal = "TPV-01",
                    TotalVentasEfectivo = docs.Where(d => d.MetodoPago == "Efectivo").Sum(d => d.Total),
                    TotalVentasTarjeta = docs.Where(d => d.MetodoPago == "Tarjeta").Sum(d => d.Total),
                    TotalIva = docs.Sum(d => d.TotalIva),
                    Observaciones = "" 
                };

                return Ok(cierre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener datos: {ex.Message}");
            }
        }

        /// <summary>
        /// Procesa el cierre y bloquea los documentos para evitar ediciones post-cierre.
        /// </summary>
        [HttpPost("ejecutar")]
        public async Task<IActionResult> EjecutarCierre([FromBody] CierreCaja cierre)
        {
            if (cierre == null) return BadRequest("Datos de cierre inválidos.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Guardar el objeto CierreCaja
                cierre.IsProcesado = true;
                _context.CierresCaja.Add(cierre);

                // 2. Marcar documentos comerciales como cerrados
                var documentos = await _context.Documentos
                    .Where(d => !d.IsContabilizado && !d.EsCompra)
                    .ToListAsync();

                foreach (var doc in documentos)
                {
                    doc.IsContabilizado = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Operación de cierre exitosa" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error interno en la transacción: {ex.Message}");
            }
        }
    }
}