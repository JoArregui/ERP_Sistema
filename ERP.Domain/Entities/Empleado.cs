using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class Empleado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string DNI { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Required]
        public string NumeroSeguridadSocial { get; set; } = string.Empty;

        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public DateTime? FechaBaja { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SalarioBrutoAnual { get; set; } // Estándar profesional

        [Column(TypeName = "decimal(18,4)")]
        public decimal SalarioBaseMensual { get; set; }

        public bool IsActivo => !FechaBaja.HasValue || FechaBaja > DateTime.Now;

        public int EmpresaId { get; set; }
        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }
    }
}