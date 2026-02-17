using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP.Domain.Entities
{
    public class Familia
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la familia es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Código visual o abreviatura para reportes (ej: "ALM" para Alimentación)
        /// </summary>
        [StringLength(10)]
        public string? CodigoInterno { get; set; }

        public bool IsActiva { get; set; } = true;

        // Relación inversa con Artículos
        public virtual ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();
    }
}