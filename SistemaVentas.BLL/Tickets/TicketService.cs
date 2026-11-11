using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaVentas.Entities.Tickets;
using System.Globalization;

namespace SistemaVentas.BLL.Tickets
{
    public class TicketService
    {
        public const string NombreMinimercado = "AzulyOro Market";
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

        static TicketService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public string GenerarTicket(TicketVenta ticket)
        {
            if (ticket is null)
                throw new ArgumentNullException(nameof(ticket));

            string carpeta = Path.Combine(Path.GetTempPath(), "SistemaVentas", "Tickets");
            Directory.CreateDirectory(carpeta);

            string rutaPdf = Path.Combine(
                carpeta,
                $"Ticket_{ticket.IdVenta}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(4, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text(ticket.NombreMinimercado)
                            .FontSize(12).Bold();
                        col.Item().AlignCenter().Text("TICKET DE VENTA").FontSize(9);
                        col.Item().PaddingTop(4)
                            .Text($"N°: {ticket.IdVenta}");
                        col.Item().Text(
                            $"Fecha: {ticket.FechaHora.ToString("dd/MM/yyyy HH:mm", Cultura)}");
                        col.Item().Text($"Cajero: {ticket.NombreCajero}");
                        col.Item().Text($"Pago: {ticket.MetodoPago}");
                    });

                    page.Content().PaddingVertical(4).Column(col =>
                    {
                        col.Item().LineHorizontal(0.5f);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(22);
                                columns.RelativeColumn();
                                columns.ConstantColumn(48);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Cant").Bold();
                                header.Cell().Text("Producto").Bold();
                                header.Cell().AlignRight().Text("Importe").Bold();
                            });

                            foreach (DetalleTicket detalle in ticket.Detalles)
                            {
                                table.Cell().Text(detalle.Cantidad.ToString(Cultura));
                                table.Cell().Text(detalle.NombreProducto);
                                table.Cell().AlignRight()
                                    .Text(detalle.Subtotal.ToString("N2", Cultura));
                            }
                        });

                        col.Item().LineHorizontal(0.5f);

                        col.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text("TOTAL").FontSize(10).Bold();
                            row.ConstantItem(60).AlignRight()
                                .Text(ticket.Total.ToString("N2", Cultura))
                                .FontSize(10).Bold();
                        });

                        if (ticket.MetodoPago == "Efectivo")
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Abonado");
                                row.ConstantItem(60).AlignRight()
                                    .Text(ticket.MontoAbonado.ToString("N2", Cultura));
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Vuelto");
                                row.ConstantItem(60).AlignRight()
                                    .Text(ticket.Vuelto.ToString("N2", Cultura));
                            });
                        }
                    });

                    page.Footer().AlignCenter()
                        .Text("¡Gracias por su compra!").FontSize(8);
                });
            })
            .GeneratePdf(rutaPdf);

            return rutaPdf;
        }
    }
}
