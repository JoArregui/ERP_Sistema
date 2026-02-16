using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class DocumentoLinea
    {
        [Key]
        public int Id { get; set; }

        public int DocumentoId { get; set; }
        [ForeignKey("DocumentoId")]
        public virtual DocumentoComercial? Documento { get; set; }

        public int ArticuloId { get; set; }
        [ForeignKey("ArticuloId")]
        public virtual Articulo? Articulo { get; set; }

        public string DescripcionArticulo { get; set; } = string.Empty; 
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public double PorcentajeIva { get; set; }

        [NotMapped]
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}