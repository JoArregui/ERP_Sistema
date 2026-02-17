using System.ComponentModel.DataAnnotations;

namespace ERP.Domain.Entities
{
    public class FamiliaArticulo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la familia es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        // Indica si esta familia está activa para nuevos artículos
        public bool IsActiva { get; set; } = true;

        // Propiedad de navegación: Una familia tiene muchos artículos
        public virtual ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();
    }
}