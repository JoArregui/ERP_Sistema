using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class Nomina
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmpleadoId { get; set; }
        [ForeignKey("EmpleadoId")]
        public virtual Empleado? Empleado { get; set; }

        public int Mes { get; set; }
        public int Anio { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SalarioBase { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Complementos { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Deducciones { get; set; } // IRPF, Seguridad Social

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalNeto => SalarioBase + Complementos - Deducciones;

        public DateTime FechaEmision { get; set; } = DateTime.Now;
        
        public bool EstaPagada { get; set; } = false;
    }
}