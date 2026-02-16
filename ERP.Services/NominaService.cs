using ERP.Data;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Services
{
    public class NominaService
    {
        private readonly ApplicationDbContext _context;

        public NominaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Nomina> GenerarNominaMensual(int empleadoId, int mes, int anio)
        {
            // 1. Validación de existencia del empleado y su empresa
            var empleado = await _context.Empleados
                .Include(e => e.Empresa)
                .FirstOrDefaultAsync(e => e.Id == empleadoId);

            if (empleado == null) 
                throw new Exception($"El empleado con ID {empleadoId} no existe en el sistema.");

            // 2. Control de duplicados profesional
            var nominaExistente = await _context.Nominas
                .AnyAsync(n => n.EmpleadoId == empleadoId && n.Mes == mes && n.Anio == anio);

            if (nominaExistente) 
                throw new Exception($"Ya existe una nómina procesada para este empleado en el periodo {mes}/{anio}.");

            // 3. Procesamiento de Control Horario para cálculo de variables
            var primerDiaMes = new DateTime(anio, mes, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            var fichajesDelMes = await _context.ControlesHorarios
                .Where(c => c.EmpleadoId == empleadoId && 
                            c.Entrada >= primerDiaMes && 
                            c.Entrada <= ultimoDiaMes && 
                            c.Salida.HasValue)
                .ToListAsync();

            decimal totalHorasRealizadas = (decimal)fichajesDelMes
                .Sum(f => (f.Salida!.Value - f.Entrada).TotalHours);

            // Lógica de Complementos: Jornada estándar 160h/mes. Horas extra a 20€.
            decimal importeHorasExtra = 0;
            if (totalHorasRealizadas > 160)
            {
                importeHorasExtra = (totalHorasRealizadas - 160) * 20;
            }

            // 4. Construcción de la Entidad Nómina
            var nuevaNomina = new Nomina
            {
                EmpleadoId = empleadoId,
                Mes = mes,
                Anio = anio,
                SalarioBase = empleado.SalarioBaseMensual,
                Complementos = importeHorasExtra,
                // Deducciones fijas del 15% (Seguridad Social + IRPF base)
                Deducciones = (empleado.SalarioBaseMensual + importeHorasExtra) * 0.15m,
                FechaEmision = DateTime.Now
            };

            _context.Nominas.Add(nuevaNomina);

            // 5. Integración con Tesorería: Generar obligación de pago
            // El neto es lo que realmente sale de la caja de la empresa
            decimal importeNeto = (nuevaNomina.SalarioBase + nuevaNomina.Complementos) - nuevaNomina.Deducciones;

            var obligacionPago = new Vencimiento
            {
                DocumentoId = 0, // 0 indica que es un gasto interno, no una factura comercial
                FechaVencimiento = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes)),
                Importe = importeNeto,
                Estado = "Pendiente",
                MetodoPago = "Transferencia",
                EmpresaId = empleado.EmpresaId, // Multi-tenant: pertenece a la empresa del empleado
                FechaPago = null
            };

            _context.Vencimientos.Add(obligacionPago);

            // 6. Persistencia atómica de datos
            await _context.SaveChangesAsync();

            return nuevaNomina;
        }
    }
}