using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Domain.Entities;
using ERP.Data;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamiliasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FamiliasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Familias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Familia>>> GetFamilias()
        {
            return await _context.Familias
                .OrderBy(f => f.Nombre)
                .ToListAsync();
        }

        // GET: api/Familias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Familia>> GetFamilia(int id)
        {
            var familia = await _context.Familias
                .Include(f => f.Articulos)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (familia == null) return NotFound();

            return familia;
        }

        // GET: api/Familias/stats
        [HttpGet("stats")]
        public async Task<ActionResult> GetFamiliasStats()
        {
            var stats = await _context.Familias
                .Select(f => new
                {
                    f.Id,
                    f.Nombre,
                    CantidadArticulos = f.Articulos.Count,
                    ValorStock = f.Articulos.Sum(a => a.Stock * a.PrecioCompra)
                })
                .ToListAsync();

            return Ok(stats);
        }

        // POST: api/Familias
        [HttpPost]
        public async Task<ActionResult<Familia>> PostFamilia(Familia familia)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Familias.Add(familia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFamilia), new { id = familia.Id }, familia);
        }

        // PUT: api/Familias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFamilia(int id, Familia familia)
        {
            if (id != familia.Id) return BadRequest("ID no coincide");

            _context.Entry(familia).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FamiliaExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Familias/5 (Baja lógica mediante el QueryFilter de IsActiva)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFamilia(int id)
        {
            var familia = await _context.Familias.FindAsync(id);
            if (familia == null) return NotFound();

            var tieneArticulos = await _context.Articulos.AnyAsync(a => a.FamiliaId == id);
            if (tieneArticulos)
            {
                return BadRequest("No se puede desactivar una familia que contiene artículos vinculados.");
            }

            familia.IsActiva = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FamiliaExists(int id) => _context.Familias.Any(e => e.Id == id);
    }
}