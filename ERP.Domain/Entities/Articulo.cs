using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class Articulo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "El código de artículo es obligatorio")]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es necesaria")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioVenta { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioCompra { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal PorcentajeIva { get; set; } = 21.00m;

        [Column(TypeName = "decimal(18,4)")]
        public decimal Stock { get; set; } = 0;

        [Column(TypeName = "decimal(18,4)")]
        public decimal StockMinimo { get; set; } = 0;

        public int? ProveedorHabitualId { get; set; }
        
        [ForeignKey("ProveedorHabitualId")]
        public virtual Proveedor? ProveedorHabitual { get; set; }

        public bool IsDescatalogado { get; set; } = false;

        // --- NUEVA PROPIEDAD PARA IMAGEN ---
        public string? ImagenUrl { get; set; }

        // --- RELACION CON FAMILIA ---
        [Required(ErrorMessage = "Debe asignar una familia al artículo")]
        public int FamiliaId { get; set; }

        [ForeignKey(nameof(FamiliaId))] 
        // CORRECCIÓN: Se cambia 'FamiliaArticulo' por 'Familia' para coincidir con la clase existente
        public virtual Familia? Familia { get; set; }
    }
}