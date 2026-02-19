using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class DocumentoLinea
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentoId { get; set; }
        
        [ForeignKey("DocumentoId")]
        public virtual DocumentoComercial? Documento { get; set; }

        public int? ArticuloId { get; set; }
        
        [ForeignKey("ArticuloId")]
        public virtual Articulo? Articulo { get; set; }

        [Required]
        public string DescripcionArticulo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioUnitario { get; set; }

        // Cambiado a decimal para evitar errores de conversión y mejorar precisión
        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeIva { get; set; }

        // Propiedad calculada para la UI y lógica de negocio
        [NotMapped]
        public decimal Subtotal => Cantidad * PrecioUnitario;

        // --- NUEVA PROPIEDAD PARA EL CIERRE ---
        [StringLength(100)]
        public string? CategoriaNombre { get; set; }
    }
}