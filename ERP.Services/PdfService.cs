using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ERP.Services
{
    public class PdfService
    {
        public PdfService()
        {
            // Ajustado a la versión actual de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarFacturaPdf(DocumentoComercial factura, Empresa empresa, Cliente? cliente)
        {
            var colorMarca = string.IsNullOrWhiteSpace(empresa.ColorHex) ? "#1e293b" : empresa.ColorHex;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(empresa.NombreComercial?.ToUpper() ?? "ERP SYSTEM").FontSize(24).ExtraBold().FontColor(colorMarca);
                            col.Item().Text(empresa.RazonSocial).FontSize(10).SemiBold();
                            col.Item().PaddingTop(2).Column(c => {
                                c.Item().Text($"CIF: {empresa.CIF}").FontSize(9);
                                c.Item().Text(empresa.Direccion ?? "Dirección no configurada").FontSize(9);
                                if (!string.IsNullOrEmpty(empresa.Telefono)) 
                                    c.Item().Text($"Teléfono: {empresa.Telefono}").FontSize(9);
                                if (!string.IsNullOrEmpty(empresa.Email)) 
                                    c.Item().Text($"Email: {empresa.Email}").FontSize(9);
                            });
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Background(colorMarca).PaddingHorizontal(10).PaddingVertical(5).Text(factura.Tipo.ToString().ToUpper())
                                .FontSize(20).ExtraBold().FontColor(Colors.White);
                            
                            col.Item().PaddingTop(10).Text(x => {
                                x.Span("Nº DOCUMENTO: ").SemiBold();
                                x.Span(factura.NumeroDocumento).Bold().FontSize(12);
                            });
                            col.Item().Text(x => {
                                x.Span("FECHA EMISIÓN: ").SemiBold();
                                x.Span(factura.Fecha.ToString("dd/MM/yyyy"));
                            });
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(0.5f); 
                            row.RelativeItem(0.5f).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(c =>
                            {
                                c.Item().Text("DATOS FISCALES DEL RECEPTOR").FontSize(8).ExtraBold().FontColor(colorMarca);
                                c.Item().PaddingTop(4).Text(cliente?.RazonSocial ?? "CLIENTE CONTADO").Bold().FontSize(11);
                                c.Item().Text($"CIF/NIF: {cliente?.CIF ?? "N/A"}");
                                c.Item().Text(cliente?.Direccion ?? "");
                                c.Item().Text($"{cliente?.CodigoPostal} {cliente?.Poblacion}");
                                c.Item().Text(cliente?.Provincia ?? "");
                            });
                        });

                        col.Item().PaddingTop(30);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("CONCEPTO / DESCRIPCIÓN");
                                header.Cell().Element(CellStyle).AlignRight().Text("CANT.");
                                header.Cell().Element(CellStyle).AlignRight().Text("PRECIO");
                                header.Cell().Element(CellStyle).AlignRight().Text("IVA");
                                header.Cell().Element(CellStyle).AlignRight().Text("SUBTOTAL");

                                // Corrección CS0311: No aplicar estilos de texto al IContainer
                                IContainer CellStyle(IContainer c) => c.PaddingVertical(8).BorderBottom(2).BorderColor(colorMarca);
                            });

                            if (factura.Lineas != null)
                            {
                                foreach (var linea in factura.Lineas)
                                {
                                    table.Cell().Element(ContentStyle).Text(linea.DescripcionArticulo);
                                    table.Cell().Element(ContentStyle).AlignRight().Text(linea.Cantidad.ToString("N2"));
                                    table.Cell().Element(ContentStyle).AlignRight().Text($"{linea.PrecioUnitario:N2} €");
                                    table.Cell().Element(ContentStyle).AlignRight().Text($"{linea.PorcentajeIva}%");
                                    
                                    decimal subtotalLinea = linea.Cantidad * linea.PrecioUnitario;
                                    table.Cell().Element(ContentStyle).AlignRight().Text($"{subtotalLinea:N2} €").SemiBold();

                                    IContainer ContentStyle(IContainer c) => c.PaddingVertical(8).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3);
                                }
                            }
                        });

                        col.Item().AlignRight().PaddingTop(20).MinWidth(200).Column(c =>
                        {
                            c.Item().Row(r => {
                                r.RelativeItem().Text("Base Imponible:").AlignRight();
                                r.RelativeItem().PaddingRight(5).Text($"{factura.BaseImponible:N2} €").AlignRight();
                            });
                            
                            c.Item().Row(r => {
                                r.RelativeItem().Text("Cuota I.V.A.:").AlignRight();
                                r.RelativeItem().PaddingRight(5).Text($"{factura.TotalIva:N2} €").AlignRight();
                            });

                            c.Item().PaddingTop(5).Background(colorMarca).Padding(8).Row(r => {
                                r.RelativeItem().Text("TOTAL DOCUMENTO").FontColor(Colors.White).ExtraBold().FontSize(12);
                                r.RelativeItem().Text($"{factura.Total:N2} €").FontColor(Colors.White).ExtraBold().FontSize(12).AlignRight();
                            });
                        });

                        if (factura.Tipo == TipoDocumento.Factura)
                        {
                            col.Item().PaddingTop(40).Column(c => {
                                c.Item().Text("INFORMACIÓN DE PAGO").FontSize(8).Bold();
                                c.Item().Text("El pago se realizará según las condiciones pactadas en su ficha de cliente.").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text(x =>
                        {
                            x.Span("Documento generado por ERP Sistema - ").FontSize(8);
                            x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                        });

                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Página ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" de ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerarCierreCajaPdf(CierreCaja cierre, Empresa empresa)
        {
            var colorMarca = string.IsNullOrWhiteSpace(empresa.ColorHex) ? "#1e293b" : empresa.ColorHex;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("RESUMEN DE CIERRE DE CAJA").FontSize(20).ExtraBold().FontColor(colorMarca);
                            col.Item().Text($"{empresa.RazonSocial}").SemiBold().FontSize(12);
                            col.Item().Text($"Terminal: {cierre.Terminal}").FontSize(9);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text(cierre.FechaCierre.ToString("f")).FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text($"ID CIERRE: #{cierre.Id}").FontSize(9).Bold();
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns => {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Element(HeaderStyle).Text("RESUMEN DE CAJA").SemiBold();
                            table.Cell().Element(HeaderStyle).AlignRight().Text("IMPORTE").SemiBold();

                            table.Cell().Element(RowStyle).Text("Ventas en Efectivo (Esperado)");
                            table.Cell().Element(RowStyle).AlignRight().Text($"{cierre.TotalVentasEfectivo:N2} €");

                            table.Cell().Element(RowStyle).Text("Ventas con Tarjeta");
                            table.Cell().Element(RowStyle).AlignRight().Text($"{cierre.TotalVentasTarjeta:N2} €");

                            table.Cell().PaddingVertical(10).BorderBottom(1).BorderColor(Colors.Black).Text("EFECTIVO REAL EN CAJA").Bold();
                            table.Cell().PaddingVertical(10).BorderBottom(1).BorderColor(Colors.Black).AlignRight().Text($"{cierre.ImporteRealEnCaja:N2} €").Bold();

                            var colorDescuadre = Math.Abs(cierre.Descuadre) < 0.01m ? Colors.Green.Medium : Colors.Red.Medium;
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(10).Text("DIFERENCIA / DESCUADRE").ExtraBold();
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(10).AlignRight().Text($"{cierre.Descuadre:N2} €").ExtraBold().FontColor(colorDescuadre);

                            IContainer HeaderStyle(IContainer c) => c.PaddingVertical(5).BorderBottom(1);
                            IContainer RowStyle(IContainer c) => c.PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
                        });

                        col.Item().PaddingTop(25).Row(row =>
                        {
                            // Tabla Categorías
                            row.RelativeItem().PaddingRight(10).Column(c => {
                                c.Item().Text("VENTAS POR CATEGORÍA").FontSize(9).ExtraBold().FontColor(colorMarca);
                                c.Item().PaddingTop(5).Table(t => {
                                    t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.RelativeColumn(); });
                                    
                                    var cats = string.IsNullOrEmpty(cierre.DataCategoriasJson) 
                                        ? new List<ResumenItem>() 
                                        : JsonSerializer.Deserialize<List<ResumenItem>>(cierre.DataCategoriasJson) ?? new List<ResumenItem>();
                                    
                                    foreach(var item in cats) {
                                        t.Cell().Element(SubRow).Text(item.Nombre).FontSize(8);
                                        t.Cell().Element(SubRow).AlignRight().Text($"{item.Total:N2} €").FontSize(8);
                                    }
                                });
                            });

                            // Tabla Usuarios
                            row.RelativeItem().PaddingLeft(10).Column(c => {
                                c.Item().Text("VENTAS POR USUARIO").FontSize(9).ExtraBold().FontColor(colorMarca);
                                c.Item().PaddingTop(5).Table(t => {
                                    t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.RelativeColumn(); });
                                    
                                    var usrs = string.IsNullOrEmpty(cierre.DataUsuariosJson) 
                                        ? new List<ResumenItem>() 
                                        : JsonSerializer.Deserialize<List<ResumenItem>>(cierre.DataUsuariosJson) ?? new List<ResumenItem>();

                                    foreach(var item in usrs) {
                                        t.Cell().Element(SubRow).Text(item.Nombre).FontSize(8);
                                        t.Cell().Element(SubRow).AlignRight().Text($"{item.Total:N2} €").FontSize(8);
                                    }
                                });
                            });

                            IContainer SubRow(IContainer c) => c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten4).PaddingVertical(3);
                        });

                        col.Item().PaddingTop(25).Text("DESGLOSE POR TIPOS DE IVA").FontSize(10).ExtraBold().Underline();
                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns => {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(h => {
                                h.Cell().Element(HeadStyle).Text("Tipo").Bold();
                                h.Cell().Element(HeadStyle).AlignRight().Text("Base Imponible").Bold();
                                h.Cell().Element(HeadStyle).AlignRight().Text("Cuota IVA").Bold();
                                IContainer HeadStyle(IContainer c) => c.BorderBottom(1).PaddingVertical(5);
                            });

                            if (cierre.Base21 > 0) {
                                table.Cell().Element(RStyle).Text("IVA General 21%");
                                table.Cell().Element(RStyle).AlignRight().Text($"{cierre.Base21:N2} €");
                                table.Cell().Element(RStyle).AlignRight().Text($"{cierre.Iva21:N2} €");
                            }
                            if (cierre.Base10 > 0) {
                                table.Cell().Element(RStyle).Text("IVA Reducido 10%");
                                table.Cell().Element(RStyle).AlignRight().Text($"{cierre.Base10:N2} €");
                                table.Cell().Element(RStyle).AlignRight().Text($"{cierre.Iva10:N2} €");
                            }
                            if (cierre.Base4 > 0) {
                                table.Cell().Element(RStyle).Text("IVA Superreducido 4%");
                                table.Cell().Element(RStyle).AlignRight().Text($"{cierre.Base4:N2} €");
                                table.Cell().Element(RStyle).AlignRight().Text($"{cierre.Iva4:N2} €");
                            }

                            table.Cell().ColumnSpan(2).PaddingVertical(10).AlignRight().Text("TOTAL IVA:").Bold();
                            table.Cell().PaddingVertical(10).AlignRight().Text($"{cierre.TotalIva:N2} €").Bold();

                            IContainer RStyle(IContainer c) => c.PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3);
                        });

                        if (!string.IsNullOrWhiteSpace(cierre.Observaciones))
                        {
                            col.Item().PaddingTop(20).Column(c => {
                                c.Item().Text("OBSERVACIONES:").FontSize(8).Bold();
                                c.Item().Padding(5).Background(Colors.Grey.Lighten4).Text(cierre.Observaciones).FontSize(9).Italic();
                            });
                        }
                    });

                    page.Footer().PaddingTop(20).Column(c => {
                        c.Item().AlignCenter().Text("Firma Responsable: ___________________________").FontSize(9);
                        c.Item().PaddingTop(10).AlignCenter().Text("Documento interno de control de caja").FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }

        private class ResumenItem
        {
            private string _nombre = "";

            [System.Text.Json.Serialization.JsonPropertyName("Categoria")]
            public string? CategoriaNombre { set { if (value != null) _nombre = value; } }
            
            [System.Text.Json.Serialization.JsonPropertyName("Usuario")]
            public string? UsuarioNombre { set { if (value != null) _nombre = value; } }

            public string Nombre { get => _nombre; set => _nombre = value; }
            public decimal Total { get; set; }
        }
    }
}