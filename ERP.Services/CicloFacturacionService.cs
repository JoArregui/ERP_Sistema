using ERP.Data;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Services
{
    public class CicloFacturacionService
    {
        private readonly ApplicationDbContext _context;

        public CicloFacturacionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentoComercial> CrearDocumento(DocumentoComercial nuevoDoc)
        {
            var empresa = await _context.Empresas.FindAsync(nuevoDoc.EmpresaId);
            if (empresa == null) throw new Exception("La empresa emisora no existe.");

            if (nuevoDoc.EsCompra)
            {
                nuevoDoc.NumeroDocumento = $"COM-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}";
            }
            else
            {
                nuevoDoc.NumeroDocumento = GenerarNumeroDocumento(nuevoDoc.Tipo, empresa);
            }

            CalcularTotales(nuevoDoc);
            await ActualizarStock(nuevoDoc);
            
            _context.Documentos.Add(nuevoDoc);
            await _context.SaveChangesAsync(); 

            if (nuevoDoc.Tipo == TipoDocumento.Factura)
            {
                GenerarVencimientoAutomatico(nuevoDoc);
                await _context.SaveChangesAsync();
            }

            return nuevoDoc;
        }

        // --- MÉTODO RECLAMADO POR LOS CONTROLADORES ---
        public async Task<DocumentoComercial> ConvertirDocumento(int documentoId, TipoDocumento nuevoTipo)
        {
            var origen = await _context.Documentos
                .Include(d => d.Lineas)
                .FirstOrDefaultAsync(d => d.Id == documentoId);

            if (origen == null) throw new Exception("Documento original no encontrado.");

            var empresa = await _context.Empresas.FindAsync(origen.EmpresaId);
            if (empresa == null) throw new Exception("Empresa no encontrada.");

            var destino = new DocumentoComercial
            {
                Tipo = nuevoTipo,
                EsCompra = origen.EsCompra,
                ClienteId = origen.ClienteId,
                ProveedorId = origen.ProveedorId,
                EmpresaId = origen.EmpresaId,
                DocumentoOrigenId = origen.Id,
                Fecha = DateTime.Now,
                NumeroFacturaProveedor = origen.NumeroFacturaProveedor,
                NumeroDocumento = origen.EsCompra
                    ? $"COM-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}"
                    : GenerarNumeroDocumento(nuevoTipo, empresa),
                Lineas = origen.Lineas.Select(l => new DocumentoLinea
                {
                    ArticuloId = l.ArticuloId,
                    DescripcionArticulo = l.DescripcionArticulo,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    PorcentajeIva = l.PorcentajeIva
                }).ToList()
            };

            CalcularTotales(destino);
            await ActualizarStock(destino);
            
            _context.Documentos.Add(destino);
            await _context.SaveChangesAsync();

            if (destino.Tipo == TipoDocumento.Factura)
            {
                GenerarVencimientoAutomatico(destino);
                await _context.SaveChangesAsync();
            }

            return destino;
        }

        private void GenerarVencimientoAutomatico(DocumentoComercial doc)
        {
            var vencimiento = new Vencimiento
            {
                DocumentoId = doc.Id,
                Importe = doc.Total,
                FechaVencimiento = doc.Fecha.AddDays(30),
                Estado = "Pendiente",
                EmpresaId = doc.EmpresaId
            };
            _context.Vencimientos.Add(vencimiento);
        }

        private void CalcularTotales(DocumentoComercial doc)
        {
            doc.BaseImponible = doc.Lineas.Sum(l => l.Cantidad * l.PrecioUnitario);
            doc.TotalIva = doc.Lineas.Sum(l => (l.Cantidad * l.PrecioUnitario) * (decimal)(l.PorcentajeIva / 100));
            doc.Total = doc.BaseImponible + doc.TotalIva;
        }

        private async Task ActualizarStock(DocumentoComercial doc)
        {
            if (doc.Tipo != TipoDocumento.Albaran && doc.Tipo != TipoDocumento.Factura) return;

            foreach (var linea in doc.Lineas)
            {
                var articulo = await _context.Articulos.FindAsync(linea.ArticuloId);
                if (articulo != null)
                {
                    if (doc.EsCompra) articulo.Stock += linea.Cantidad;
                    else articulo.Stock -= linea.Cantidad;
                }
            }
        }

        private string GenerarNumeroDocumento(TipoDocumento tipo, Empresa empresa)
        {
            string prefijo = tipo switch
            {
                TipoDocumento.Presupuesto => "PRE",
                TipoDocumento.Pedido => "PED",
                TipoDocumento.Albaran => "ALB",
                TipoDocumento.Factura => "FAC",
                _ => "DOC"
            };
            return $"{prefijo}-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}";
        }
    }
}