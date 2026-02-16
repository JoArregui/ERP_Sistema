namespace ERP.Api.Models
{
    public class DashboardDTO
    {
        // Totales para las Cards
        public decimal TotalVentas { get; set; }
        public decimal TotalCompras { get; set; }
        public decimal TotalNominas { get; set; }
        public decimal BeneficioNeto => TotalVentas - (TotalCompras + TotalNominas);

        // Alertas de Tesorería
        public int FacturasPendientesCobro { get; set; }
        public decimal ImportePendienteCobro { get; set; }
        public int FacturasVencidas { get; set; } // Facturas que han pasado su fecha límite

        // Inventario
        public int ArticulosStockBajo { get; set; }

        // Datos para Gráficos (Ventas por Mes)
        public List<GraficoVentasMes>? VentasMensuales { get; set; }
    }

    public class GraficoVentasMes
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Importe { get; set; }
    }
}