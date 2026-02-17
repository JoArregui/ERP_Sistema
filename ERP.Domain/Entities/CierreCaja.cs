using System;

namespace ERP.Domain.Entities
{
    public class CierreCaja
    {
        public int Id { get; set; }
        public DateTime FechaCierre { get; set; } = DateTime.Now;
        public string Terminal { get; set; } = "TPV-01";
        
        public decimal TotalVentasEfectivo { get; set; }
        public decimal TotalVentasTarjeta { get; set; }
        public decimal TotalIva { get; set; }
        
        public decimal ImporteRealEnCaja { get; set; }
        public decimal Descuadre => ImporteRealEnCaja - TotalVentasEfectivo;
        
        // Corregido CS8618: Permitimos null o inicializamos
        public string? Observaciones { get; set; } 
        public bool IsProcesado { get; set; } = false;
    }
}