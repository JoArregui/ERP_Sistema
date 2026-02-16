using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CIF { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        public string? Email { get; set; }

        public string? Telefono { get; set; }

        // Propiedad necesaria para el filtro en ApplicationDbContext
        public bool IsActivo { get; set; } = true;

        // Relación con la Empresa (Multi-tenant)
        public int EmpresaId { get; set; }
        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }
    }
}