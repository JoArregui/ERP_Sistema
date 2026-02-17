using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Domain.Entities;
using ERP.Domain.DTOs;
using ERP.Data;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticulosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArticulosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- MÉTODOS DE SINCRONIZACIÓN PARA SCANPAL (OFFLINE-FIRST) ---

        // GET: api/Articulos/scanpal/sincronizar
        // Descarga el catálogo completo para la BD Local de Scanpal
        [HttpGet("scanpal/sincronizar")]
        public async Task<ActionResult> GetSincronizacionTotal()
        {
            var articulos = await _context.Articulos
                .Where(a => !a.IsDescatalogado)
                .Select(a => new {
                    a.Id,
                    a.Codigo,
                    a.Descripcion,
                    a.Stock,
                    a.PrecioVenta,
                    a.PrecioCompra,
                    Familia = a.Familia != null ? a.Familia.Nombre : ""
                })
                .ToListAsync();

            return Ok(articulos);
        }

        // GET: api/Articulos/scanpal/documentos-pendientes
        // Descarga albaranes de compra y venta para puntear en Scanpal
        [HttpGet("scanpal/documentos-pendientes")]
        public async Task<ActionResult> GetDocumentosPendientes()
        {
            var documentos = await _context.Documentos
                .Where(d => !d.IsContabilizado && (d.Tipo == TipoDocumento.Albaran || d.Tipo == TipoDocumento.Pedido))
                .Select(d => new {
                    d.Id,
                    d.Tipo,
                    d.NumeroDocumento,
                    d.EsCompra,
                    d.Fecha,
                    Entidad = d.EsCompra ? (d.Proveedor != null ? d.Proveedor.RazonSocial : "Proveedor Desconocido") 
                                         : (d.Cliente != null ? d.Cliente.NombreComercial : "Cliente Desconocido"),
                    Lineas = d.Lineas.Select(l => new {
                        l.ArticuloId,
                        l.Cantidad,
                        CodigoArticulo = l.Articulo != null ? l.Articulo.Codigo : ""
                    })
                })
                .ToListAsync();

            return Ok(documentos);
        }

        // POST: api/Articulos/scanpal/subir-inventario
        [HttpPost("scanpal/subir-inventario")]
        public async Task<IActionResult> SincronizarInventarioMasivo([FromBody] List<AjusteStockDTO> lecturas)
        {
            if (lecturas == null || !lecturas.Any()) return BadRequest("No hay lecturas para procesar.");

            var empresaId = await _context.Empresas.Select(e => e.Id).FirstOrDefaultAsync();
            var fechaSincro = DateTime.Now;

            foreach (var lectura in lecturas)
            {
                var articulo = await _context.Articulos.FirstOrDefaultAsync(a => a.Codigo == lectura.CodigoBarras);
                if (articulo != null)
                {
                    decimal stockAnterior = articulo.Stock;
                    articulo.Stock = lectura.CantidadReal;

                    _context.Documentos.Add(new DocumentoComercial
                    {
                        Tipo = TipoDocumento.Albaran, 
                        Fecha = fechaSincro,
                        NumeroDocumento = $"INV-{fechaSincro:yyyyMMdd}-{lectura.TerminalId}",
                        EmpresaId = empresaId,
                        EsCompra = (lectura.CantidadReal > stockAnterior),
                        Observaciones = $"Sincro Scanpal (Inventario): {stockAnterior} -> {lectura.CantidadReal}",
                        IsContabilizado = true,
                        BaseImponible = 0, TotalIva = 0, Total = 0
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Sincronización masiva de inventario completada." });
        }

        // --- MÉTODOS ESTÁNDAR Y REPORTES ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Articulo>>> GetArticulos()
        {
            return await _context.Articulos.Include(a => a.Familia).Include(a => a.ProveedorHabitual).ToListAsync();
        }

        [HttpGet("analisis-rentabilidad")]
        public async Task<ActionResult<IEnumerable<AnalisisRentabilidadDTO>>> GetRentabilidad()
        {
            return await _context.Articulos
                .Include(a => a.Familia)
                .Where(a => !a.IsDescatalogado)
                .Select(a => new AnalisisRentabilidadDTO
                {
                    Codigo = a.Codigo,
                    Descripcion = a.Descripcion,
                    Familia = a.Familia != null ? a.Familia.Nombre : "Sin Familia",
                    PrecioCompra = a.PrecioCompra,
                    PrecioVenta = a.PrecioVenta,
                    StockActual = a.Stock
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Articulo>> GetArticulo(int id)
        {
            var articulo = await _context.Articulos.Include(a => a.Familia).Include(a => a.ProveedorHabitual).FirstOrDefaultAsync(a => a.Id == id);
            if (articulo == null) return NotFound();
            return articulo;
        }

        [HttpGet("by-codigo/{codigo}")]
        public async Task<ActionResult<Articulo>> GetArticuloByCodigo(string codigo)
        {
            var articulo = await _context.Articulos.FirstOrDefaultAsync(a => a.Codigo == codigo);
            if (articulo == null) return NotFound(new { message = "Artículo no encontrado" });
            return articulo;
        }

        [HttpPost]
        public async Task<ActionResult<Articulo>> PostArticulo(Articulo articulo)
        {
            _context.Articulos.Add(articulo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetArticulo), new { id = articulo.Id }, articulo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticulo(int id, Articulo articulo)
        {
            if (id != articulo.Id) return BadRequest();
            _context.Entry(articulo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticulo(int id)
        {
            var articulo = await _context.Articulos.FindAsync(id);
            if (articulo == null) return NotFound();
            articulo.IsDescatalogado = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ArticuloExists(int id) => _context.Articulos.Any(e => e.Id == id);
    }
}