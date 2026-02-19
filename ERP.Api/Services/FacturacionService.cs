using ERP.Data;
using ERP.Domain.Entities;
using ERP.API.Services; // Asegúrate de que apunta a la carpeta de StockService
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services
{
    public class FacturacionService
    {
        private readonly ApplicationDbContext _context;
        private readonly StockService _stockService;

        public FacturacionService(ApplicationDbContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        public async Task<bool> RegistrarFacturaVentaAsync(DocumentoComercial factura)
        {
            // Usamos una transacción para que si falla el stock, no se guarde la factura
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Validaciones mínimas de negocio
                if (factura.Lineas == null || !factura.Lineas.Any())
                    throw new Exception("La factura no tiene líneas.");

                // 2. Guardar el documento (Cabecera y Líneas)
                _context.Documentos.Add(factura);
                await _context.SaveChangesAsync(); 

                // 3. Delegar la actualización de Stock al servicio especializado
                // Este servicio ya crea los MovimientoStock con tus campos correctos
                // (TipoMovimiento, ReferenciaDocumento, etc.)
                await _stockService.ProcesarMovimientoStock(factura.Id);

                // 4. Consolidar cambios
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log del error (opcional)
                Console.WriteLine($"Error en facturación: {ex.Message}");
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}