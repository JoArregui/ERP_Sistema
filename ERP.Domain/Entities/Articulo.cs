using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class Articulo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal Precio { get; set; }

        // Nuevo campo para control de inventario
        [Column(TypeName = "decimal(18,4)")]
        public decimal Stock { get; set; } = 0;

        public bool IsDescatalogado { get; set; } = false;
    }
}