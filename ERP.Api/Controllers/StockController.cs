using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Data;
using ERP.Domain.DTOs;
using ERP.Domain.Entities;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene los indicadores clave de valoración de inventario.
        /// Corregido para usar la propiedad 'Stock' de la entidad Articulo.
        /// </summary>
        [HttpGet("valoracion-dashboard")]
        public async Task<ActionResult<ValoracionStockDTO>> GetValoracionDashboard()
        {
            // Obtenemos solo artículos que tienen existencias
            var articulos = await _context.Articulos
                .Where(a => a.Stock > 0)
                .ToListAsync();

            var dto = new ValoracionStockDTO
            {
                // Valor total: Sumatoria de (Stock * PrecioCompra)
                ValorTotalAlmacen = articulos.Sum(a => a.Stock * a.PrecioCompra),
                
                TotalArticulosDiferentes = articulos.Count,
                
                // Cantidad total de unidades (físicas)
                CantidadTotalUnidades = (double)articulos.Sum(a => a.Stock),
                
                // Ranking de los 5 artículos que representan más dinero inmovilizado
                TopArticulosMasValiosos = articulos
                    .OrderByDescending(a => a.Stock * a.PrecioCompra)
                    .Take(5)
                    .Select(a => new ArticuloValoradoDTO
                    {
                        Codigo = a.Codigo,
                        Descripcion = a.Descripcion,
                        Stock = (double)a.Stock, // Casting a double para el DTO visual
                        PMP = a.PrecioCompra
                    })
                    .ToList()
            };

            return Ok(dto);
        }
    }
}