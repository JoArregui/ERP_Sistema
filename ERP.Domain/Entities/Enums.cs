namespace ERP.Domain.Entities
{
    public enum TipoDocumento
    {
        Presupuesto,
        Pedido,
        Albaran,
        Factura,
        AjusteStock // <--- Crucial para Scanpal y auditoría de almacén
    }
}