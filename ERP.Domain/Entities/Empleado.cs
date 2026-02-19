using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class Empleado
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI/NIE es obligatorio")]
        [StringLength(20)]
        public string DNI { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Formato de email incorrecto")]
        public string? Email { get; set; }
        
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El Nº de Seguridad Social es obligatorio")]
        public string NumeroSeguridadSocial { get; set; } = string.Empty;

        // --- ACCESO KIOSKO ---
        [Required(ErrorMessage = "El PIN de acceso es obligatorio para el fichaje")]
        [StringLength(10)]
        public string PinAcceso { get; set; } = string.Empty;

        // --- DATOS CONTRACTUALES ---
        public string? Cargo { get; set; } 
        public string? Departamento { get; set; } 
        
        [Required]
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public DateTime? FechaBaja { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SalarioBrutoAnual { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SalarioBaseMensual { get; set; }

        // --- PAGO Y BANCO ---
        [StringLength(34)]
        public string? IBAN { get; set; } 

        // --- LÓGICA DE ESTADO ---
        [NotMapped] 
        public bool IsActivo => !FechaBaja.HasValue || FechaBaja > DateTime.Now;

        // --- RELACIONES ---
        public int EmpresaId { get; set; }
        
        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        // Colecciones para navegación
        public virtual ICollection<Nomina> Nominas { get; set; } = new List<Nomina>();
        public virtual ICollection<ControlHorario> ControlesHorarios { get; set; } = new List<ControlHorario>();
    }
}