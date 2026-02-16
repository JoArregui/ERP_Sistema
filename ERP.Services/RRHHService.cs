using ERP.Data;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class RRHHService
    {
        private readonly ApplicationDbContext _context;

        public RRHHService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarEntrada(int empleadoId)
        {
            var registro = new ControlHorario
            {
                EmpleadoId = empleadoId,
                Entrada = DateTime.Now
            };

            _context.ControlesHorarios.Add(registro);
            await _context.SaveChangesAsync();
        }

        public async Task RegistrarSalida(int empleadoId)
        {
            var ultimoRegistro = await _context.ControlesHorarios
                .Where(c => c.EmpleadoId == empleadoId && c.Salida == null)
                .OrderByDescending(c => c.Entrada)
                .FirstOrDefaultAsync();

            if (ultimoRegistro != null)
            {
                ultimoRegistro.Salida = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}