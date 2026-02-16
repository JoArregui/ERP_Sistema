using System.ComponentModel.DataAnnotations;

namespace ERP.Domain.Entities
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CIF { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NombreContacto { get; set; }

        public string? Email { get; set; }

        public string? Telefono { get; set; }

        // Diferenciamos si nos vende producto (Mercadería) o servicios (Luz, Alquiler)
        public bool EsAcreedor { get; set; } 

        public bool IsActivo { get; set; } = true;
    }
}