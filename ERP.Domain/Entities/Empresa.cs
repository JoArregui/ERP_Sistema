using System.ComponentModel.DataAnnotations;

namespace ERP.Domain.Entities
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre comercial es obligatorio")]
        [StringLength(100)]
        public string NombreComercial { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razón social es legalmente necesaria")]
        [StringLength(150)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CIF/NIF es imprescindible")]
        [StringLength(20)]
        public string CIF { get; set; } = string.Empty;

        public string? Direccion { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Poblacion { get; set; }
        public string? Provincia { get; set; }
        
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Web { get; set; }

        // --- DATOS MERCANTILES (Para validez legal en facturas) ---
        public string? RegistroMercantil { get; set; } // Ej: Registro Mercantil de Madrid, Tomo...

        // --- ATRIBUTOS VISUALES ---
        public string? LogoBase64 { get; set; } 
        public string ColorHex { get; set; } = "#3498db"; 
        public string? Eslogan { get; set; }
        // --------------------------

        // --- CONFIGURACIÓN DE NEGOCIO ---
        [Required]
        public string SerieFacturacion { get; set; } = "2026";
        public int UltimoNumeroFactura { get; set; } = 0;
        public decimal IvaDefecto { get; set; } = 21m;

        public bool IsActiva { get; set; } = true;

        // --- AUDITORÍA ---
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public DateTime? UltimaModificacion { get; set; }
    }
}