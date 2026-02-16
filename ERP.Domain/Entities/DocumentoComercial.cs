using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Domain.Entities
{
    public class DocumentoComercial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TipoDocumento Tipo { get; set; }

        // Indica si es una Compra (true) o una Venta (false)
        public bool EsCompra { get; set; } = false;

        [Required]
        public string NumeroDocumento { get; set; } = string.Empty; 

        // Campo opcional para registrar el número de factura que nos envía el proveedor
        public string? NumeroFacturaProveedor { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Relación con Empresa (La empresa gestora que emite o recibe)
        [Required]
        public int EmpresaId { get; set; }
        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        // Relación opcional con Cliente (Para Ventas)
        public int? ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        // Relación opcional con Proveedor (Para Compras)
        public int? ProveedorId { get; set; }
        [ForeignKey("ProveedorId")]
        public virtual Proveedor? Proveedor { get; set; }

        // Trazabilidad
        public int? DocumentoOrigenId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal BaseImponible { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIva { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }

        public virtual ICollection<DocumentoLinea> Lineas { get; set; } = new List<DocumentoLinea>();
    }
}