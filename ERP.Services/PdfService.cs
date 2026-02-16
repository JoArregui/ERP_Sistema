using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ERP.Domain.Entities;
using System.Linq;

namespace ERP.Services
{
    public class PdfService
    {
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarFacturaPdf(DocumentoComercial factura, Empresa empresa, Cliente? cliente)
        {
            // Convertimos el color Hex de la DB a un formato que QuestPDF entienda
            var colorMarca = empresa.ColorHex ?? "#000000";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    // --- CABECERA DINÁMICA ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(empresa.NombreComercial.ToUpper()).FontSize(22).SemiBold().FontColor(colorMarca);
                            col.Item().Text(empresa.RazonSocial).FontSize(9).Italic();
                            col.Item().Text($"CIF: {empresa.CIF}").FontSize(9);
                            col.Item().Text(empresa.Direccion ?? "").FontSize(9);
                            if (!string.IsNullOrEmpty(empresa.Telefono)) 
                                col.Item().Text($"Telf: {empresa.Telefono}").FontSize(9);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Background(colorMarca).PaddingHorizontal(10).Text(factura.Tipo.ToString().ToUpper())
                                .FontSize(20).SemiBold().FontColor(Colors.White);
                            
                            col.Item().PaddingTop(5).Text($"Nº: {factura.NumeroDocumento}").SemiBold();
                            col.Item().Text($"Fecha: {factura.Fecha:dd/MM/yyyy}");
                        });
                    });

                    // --- CUERPO ---
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        // Datos del receptor con un diseño de "Caja"
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(0.5f); // Espacio vacío a la izquierda
                            row.RelativeItem(0.5f).Border(0.5f).BorderColor(colorMarca).Padding(10).Column(c =>
                            {
                                c.Item().Text("CLIENTE").FontSize(8).SemiBold().FontColor(colorMarca);
                                c.Item().Text(cliente?.Nombre ?? "Cliente General").SemiBold();
                                c.Item().Text($"NIF: {cliente?.CIF}");
                                c.Item().Text(cliente?.Direccion ?? "");
                            });
                        });

                        col.Item().PaddingTop(20);

                        // Tabla con estilo visual mejorado
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("CONCEPTO");
                                header.Cell().Element(CellStyle).AlignRight().Text("CANT.");
                                header.Cell().Element(CellStyle).AlignRight().Text("PRECIO");
                                header.Cell().Element(CellStyle).AlignRight().Text("TOTAL");

                                IContainer CellStyle(IContainer c) => c.PaddingVertical(5).BorderBottom(1.5f).BorderColor(colorMarca).DefaultTextStyle(x => x.SemiBold());
                            });

                            if (factura.Lineas != null)
                            {
                                foreach (var linea in factura.Lineas)
                                {
                                    table.Cell().Element(ContentStyle).Text(linea.DescripcionArticulo);
                                    table.Cell().Element(ContentStyle).AlignRight().Text(linea.Cantidad.ToString("N2"));
                                    table.Cell().Element(ContentStyle).AlignRight().Text($"{linea.PrecioUnitario:N2} €");
                                    table.Cell().Element(ContentStyle).AlignRight().Text($"{(linea.Cantidad * linea.PrecioUnitario):N2} €");

                                    IContainer ContentStyle(IContainer c) => c.PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3);
                                }
                            }
                        });

                        // Totales destacados
                        col.Item().AlignRight().PaddingTop(20).Column(c =>
                        {
                            c.Item().Text($"Base Imponible: {factura.BaseImponible:N2} €");
                            c.Item().Text($"I.V.A.: {factura.TotalIva:N2} €");
                            c.Item().Background(Colors.Grey.Lighten4).Padding(5)
                                .Text($"TOTAL FACTURA: {factura.Total:N2} €").FontSize(14).SemiBold().FontColor(colorMarca);
                        });
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Página ").FontSize(8);
                        x.CurrentPageNumber().FontSize(8);
                    });
                });
            }).GeneratePdf();
        }
    }
}