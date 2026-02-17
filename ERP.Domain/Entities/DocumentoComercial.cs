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

        // Relación con Empresa
        [Required]
        public int EmpresaId { get; set; }
        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        // Relación opcional con Cliente (Ventas)
        public int? ClienteId { get; set; }
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        // Relación opcional con Proveedor (Compras)
        public int? ProveedorId { get; set; }
        [ForeignKey("ProveedorId")]
        public virtual Proveedor? Proveedor { get; set; }

        public int? DocumentoOrigenId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal BaseImponible { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIva { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }

        // --- CAMPOS NUEVOS PARA CIERRE Y ERP ---
        
        /// <summary>
        /// Flag para indicar que este documento ya pasó por un Cierre de Caja.
        /// </summary>
        public bool IsContabilizado { get; set; } = false;

        /// <summary>
        /// Almacena el método: "Efectivo", "Tarjeta", "Transferencia".
        /// </summary>
        public string MetodoPago { get; set; } = "Efectivo";

        public virtual ICollection<DocumentoLinea> Lineas { get; set; } = new List<DocumentoLinea>();
    }
}