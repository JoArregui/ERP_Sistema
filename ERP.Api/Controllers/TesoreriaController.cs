using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERP.Data;
using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TesoreriaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TesoreriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método privado para garantizar el aislamiento Multi-tenant
        private int GetEmpresaId()
        {
            var claim = User.FindFirst("EmpresaId")?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }

        [HttpGet("pendientes-cobro")]
        public async Task<IActionResult> GetPendientesCobro()
        {
            int empresaId = GetEmpresaId();
            
            // Obtenemos vencimientos de ventas (cobros) pendientes
            var cobros = await _context.Vencimientos
                .Include(v => v.Documento)
                .ThenInclude(d => d!.Cliente)
                .Where(v => v.EmpresaId == empresaId && 
                            v.Estado == "Pendiente" && 
                            v.Documento != null && 
                            !v.Documento.EsCompra)
                .OrderBy(v => v.FechaVencimiento)
                .ToListAsync();

            return Ok(cobros);
        }

        [HttpGet("pendientes-pago")]
        public async Task<IActionResult> GetPendientesPago()
        {
            int empresaId = GetEmpresaId();

            // Obtenemos obligaciones: Facturas de proveedores y Nóminas
            var pagos = await _context.Vencimientos
                .Include(v => v.Documento)
                .ThenInclude(d => d!.Proveedor)
                .Where(v => v.EmpresaId == empresaId && 
                            v.Estado == "Pendiente" && 
                            (v.Documento == null || v.Documento.EsCompra))
                .OrderBy(v => v.FechaVencimiento)
                .ToListAsync();

            return Ok(pagos);
        }

        [HttpPost("liquidar/{id}")]
        public async Task<IActionResult> LiquidarVencimiento(int id, [FromQuery] string metodoPago)
        {
            var vencimiento = await _context.Vencimientos
                .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == GetEmpresaId());

            if (vencimiento == null)
                return NotFound("Registro de tesorería no encontrado o sin permisos.");

            if (vencimiento.Estado == "Pagado")
                return BadRequest("Este vencimiento ya fue liquidado anteriormente.");

            // Actualización de estado profesional
            vencimiento.Estado = "Pagado";
            vencimiento.FechaPago = DateTime.Now;
            vencimiento.MetodoPago = string.IsNullOrEmpty(metodoPago) ? "Efectivo/Caja" : metodoPago;

            await _context.SaveChangesAsync();

            return Ok(new { 
                Status = "Success", 
                Message = $"Vencimiento {id} marcado como pagado el {vencimiento.FechaPago}." 
            });
        }
    }
}