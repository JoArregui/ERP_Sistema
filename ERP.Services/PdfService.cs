using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ERP.Domain.Entities;
using System;
using System.Linq;

namespace ERP.Services
{
    public class PdfService
    {
        public PdfService()
        {
            // Configuración de licencia para uso comunitario
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarFacturaPdf(DocumentoComercial factura, Empresa empresa, Cliente? cliente)
        {
            // Fallback de seguridad para color de marca
            var colorMarca = string.IsNullOrWhiteSpace(empresa.ColorHex) ? "#1e293b" : empresa.ColorHex;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    
                    // Configuración de la fuente predeterminada
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    // --- CABECERA (Header) ---
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

                    // --- CUERPO DEL DOCUMENTO (Content) ---
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        // Bloque de datos del cliente
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

                        // Tabla de artículos/servicios
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);    // Concepto
                                columns.RelativeColumn(1);    // Cantidad
                                columns.RelativeColumn(1.5f); // Precio
                                columns.RelativeColumn(1);    // IVA
                                columns.RelativeColumn(1.5f); // Subtotal
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("CONCEPTO / DESCRIPCIÓN");
                                header.Cell().Element(CellStyle).AlignRight().Text("CANT.");
                                header.Cell().Element(CellStyle).AlignRight().Text("PRECIO");
                                header.Cell().Element(CellStyle).AlignRight().Text("IVA");
                                header.Cell().Element(CellStyle).AlignRight().Text("SUBTOTAL");

                                IContainer CellStyle(IContainer c) => c.PaddingVertical(8).BorderBottom(2).BorderColor(colorMarca).DefaultTextStyle(x => x.ExtraBold().FontSize(9));
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

                        // Bloque de Totales
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

                        // Información adicional (Pie de página interno)
                        if (factura.Tipo == TipoDocumento.Factura)
                        {
                            col.Item().PaddingTop(40).Column(c => {
                                c.Item().Text("INFORMACIÓN DE PAGO").FontSize(8).Bold();
                                c.Item().Text("El pago se realizará según las condiciones pactadas en su ficha de cliente.").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        }
                    });

                    // --- PIE DE PÁGINA (Footer) ---
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
    }
}