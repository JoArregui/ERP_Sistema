using System.ComponentModel.DataAnnotations;

namespace ERP.Domain.Entities
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string NombreComercial { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CIF { get; set; } = string.Empty;

        public string? Direccion { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; } // Añadido para contacto

        // --- ATRIBUTOS VISUALES ---
        public string? LogoBase64 { get; set; } // Para guardar el logo en formato imagen
        public string ColorHex { get; set; } = "#3498db"; // Color principal (Default: Azul)
        public string? Eslogan { get; set; }
        // --------------------------

        public string SerieFacturacion { get; set; } = "A";
        public int UltimoNumeroFactura { get; set; } = 0;

        public bool IsActiva { get; set; } = true;
    }
}