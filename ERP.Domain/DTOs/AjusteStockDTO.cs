namespace ERP.Domain.DTOs
{
    public class AjusteStockDTO
    {
        public string CodigoBarras { get; set; } = string.Empty;
        public decimal CantidadReal { get; set; }
        public string TerminalId { get; set; } = "Scanpal-Mobile";
    }
}