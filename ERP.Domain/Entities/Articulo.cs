using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class Articulo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de artículo es obligatorio")]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es necesaria")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioVenta { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioCompra { get; set; } // Para calcular margen de beneficio

        [Column(TypeName = "decimal(18,2)")]
        public decimal PorcentajeIva { get; set; } = 21.00m;

        [Column(TypeName = "decimal(18,4)")]
        public decimal Stock { get; set; } = 0;

        public bool IsDescatalogado { get; set; } = false;

        // --- RELACIÓN CON FAMILIA ---
        [Required(ErrorMessage = "Debe asignar una familia al artículo")]
        public int FamiliaId { get; set; }

        [ForeignKey("FamiliaId")]
        public virtual FamiliaArticulo? Familia { get; set; }
    }
}