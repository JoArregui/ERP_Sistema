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

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        public TipoDocumento Tipo { get; set; }

        /// <summary>
        /// Define la dirección del flujo: true para Compras (Entradas), false para Ventas (Salidas).
        /// </summary>
        public bool EsCompra { get; set; } = false;

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [StringLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty; 

        /// <summary>
        /// Referencia del albarán o factura física entregada por el proveedor.
        /// </summary>
        [StringLength(100)]
        public string? NumeroAlbaran { get; set; }

        /// <summary>
        /// Referencia externa opcional.
        /// </summary>
        public string? NumeroFacturaProveedor { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha real en la que se confirma la entrada de mercancía en almacén.
        /// </summary>
        public DateTime? FechaRecepcion { get; set; }

        // --- RELACIÓN CON EMPRESA (Multi-empresa) ---
        [Required]
        public int EmpresaId { get; set; }
        
        [ForeignKey("EmpresaId")]
        public virtual Empresa? Empresa { get; set; }

        // --- RELACIÓN CON CLIENTE (Ventas) ---
        public int? ClienteId { get; set; }
        
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        // --- RELACIÓN CON PROVEEDOR (Compras) ---
        public int? ProveedorId { get; set; }
        
        [ForeignKey("ProveedorId")]
        public virtual Proveedor? Proveedor { get; set; }

        /// <summary>
        /// ID del documento del que proviene (ej. Pedido -> Albarán -> Factura)
        /// </summary>
        public int? DocumentoOrigenId { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal BaseImponible { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIva { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }

        // --- ESTADOS Y CONTROL ---
        
        /// <summary>
        /// Indica si el documento ha sido cerrado, contabilizado o pasado por caja.
        /// </summary>
        public bool IsContabilizado { get; set; } = false;

        /// <summary>
        /// Método utilizado: "Efectivo", "Tarjeta", "Transferencia".
        /// </summary>
        public string MetodoPago { get; set; } = "Efectivo";

        /// <summary>
        /// Notas internas o detalles del ajuste. 
        /// </summary>
        public string? Observaciones { get; set; } 

        // --- RELACIÓN CON LAS LÍNEAS DEL DOCUMENTO ---
        public virtual ICollection<DocumentoLinea> Lineas { get; set; } = new List<DocumentoLinea>();
    }
}