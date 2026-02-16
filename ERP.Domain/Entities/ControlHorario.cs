using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class ControlHorario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmpleadoId { get; set; }
        
        [ForeignKey("EmpleadoId")]
        public virtual Empleado? Empleado { get; set; }

        public DateTime Entrada { get; set; }
        public DateTime? Salida { get; set; }

        // Coordenadas o IP (opcional para teletrabajo)
        public string? Ubicacion { get; set; }

        [NotMapped]
        public TimeSpan? TotalHoras => Salida.HasValue ? Salida - Entrada : null;
    }
}