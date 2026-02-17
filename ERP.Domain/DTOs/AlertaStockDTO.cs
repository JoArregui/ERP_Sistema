namespace ERP.Domain.DTOs
{
    public class AlertaStockDTO
    {
        public int ArticuloId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal CantidadSugerida => StockMinimo * 1.5m - StockActual; // Reponer hasta superar el mínimo
        public string ProveedorNombre { get; set; } = "Sin Proveedor";
        public string EmailProveedor { get; set; } = string.Empty;
    }
}