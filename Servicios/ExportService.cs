using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using proyectoTienda.Models;

namespace proyectoTienda.Servicios
{
  public class ExportService : IExportService
  {
    public byte[] GenerateAllPedidosPdf(List<Pedido> pedidos)
    {
      QuestPDF.Settings.License = LicenseType.Community;

      var document = Document.Create(container =>
      {
        container.Page(page =>
              {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                // Encabezado del documento
                page.Header()
                          .AlignCenter()
                          .Text("TEXTIL SALAS - REPORTE DE PEDIDOS")
                          .FontSize(18).Bold();

                // Contenido principal
                page.Content()
                          .PaddingVertical(1, Unit.Centimetre)
                          .Column(column =>
                          {
                            foreach (var pedido in pedidos)
                            {
                              // Información del pedido
                              column.Item().Background(Colors.Grey.Lighten3)
                                    .Padding(10)
                                    .Column(pedidoColumn =>
                                    {
                                      pedidoColumn.Item().Grid(grid =>
                                        {
                                            grid.Columns(3);
                                            grid.Spacing(10);

                                            grid.Item().Text($"ID Pedido: {pedido.IDPedido}").Bold();
                                            grid.Item().Text($"Fecha: {pedido.FechaPedido.ToString("dd/MM/yyyy")}").Bold();
                                            grid.Item().Text($"Email Cliente: {pedido.Cliente?.Email ?? "N/A"}").Bold();

                                            grid.Item().Text($"Tipo Entrega: {pedido.TipoEntrega}").Bold();
                                            grid.Item().Text($"Estado: {pedido.Estado}").Bold();
                                            grid.Item().Text($"Total: S/{pedido.Total.ToString("0.00")}").Bold();
                                          });

                                      // Detalles del pedido
                                      if (pedido.DetallesPedidos != null && pedido.DetallesPedidos.Any())
                                      {
                                        pedidoColumn.Item().PaddingTop(10).Table(table =>
                                          {
                                              table.ColumnsDefinition(columns =>
                                                {
                                                    columns.RelativeColumn(3); // Producto
                                                    columns.RelativeColumn();  // Cantidad
                                                    columns.RelativeColumn();  // Precio Unitario
                                                    columns.RelativeColumn();  // Subtotal
                                                  });

                                              table.Header(header =>
                                                {
                                                    header.Cell().Text("Producto").Bold();
                                                    header.Cell().Text("Cantidad").Bold();
                                                    header.Cell().Text("Precio Unitario").Bold();
                                                    header.Cell().Text("Subtotal").Bold();
                                                  });

                                              foreach (var detalle in pedido.DetallesPedidos)
                                              {
                                                table.Cell().Text(detalle.Producto?.Nombre ?? "Producto no disponible");
                                                table.Cell().Text(detalle.Cantidad.ToString());
                                                table.Cell().Text($"S/{detalle.Producto?.PrecioActual.ToString("0.00") ?? "0.00"}");
                                                table.Cell().Text($"S/{detalle.Subtotal.ToString("0.00")}");
                                              }
                                            });
                                      }
                                    });

                              // Espacio entre pedidos
                              column.Item().PaddingBottom(15);
                            }
                          });
              });
      });

      return document.GeneratePdf();
    }

    public byte[] GenerateAllPedidosExcel(List<Pedido> pedidos)
    {
      using (var workbook = new XLWorkbook())
      {
        var worksheet = workbook.Worksheets.Add("Pedidos");

        // Encabezado principal
        worksheet.Cell(1, 1).Value = "TEXTIL SALAS - REPORTE DE PEDIDOS";
        worksheet.Range(1, 1, 1, 7).Merge().Style.Font.Bold = true;
        worksheet.Range(1, 1, 1, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        int currentRow = 3;

        foreach (var pedido in pedidos)
        {
          // Encabezado del pedido
          worksheet.Cell(currentRow, 1).Value = "ID Pedido:";
          worksheet.Cell(currentRow, 2).Value = pedido.IDPedido.ToString();
          worksheet.Cell(currentRow, 5).Value = "Fecha:";
          worksheet.Cell(currentRow, 6).Value = pedido.FechaPedido.ToString("dd/MM/yyyy");
          currentRow++;

          worksheet.Cell(currentRow, 1).Value = "Email Cliente:";
          worksheet.Cell(currentRow, 2).Value = pedido.Cliente?.Email ?? "N/A";
          worksheet.Cell(currentRow, 5).Value = "Tipo Entrega:";
          worksheet.Cell(currentRow, 6).Value = pedido.TipoEntrega;
          currentRow++;

          worksheet.Cell(currentRow, 1).Value = "Estado:";
          worksheet.Cell(currentRow, 2).Value = pedido.Estado;
          worksheet.Cell(currentRow, 5).Value = "Total:";
          worksheet.Cell(currentRow, 6).Value = $"S/{pedido.Total.ToString("0.00")}";
          currentRow++;

          // Encabezados de detalles
          worksheet.Cell(currentRow, 1).Value = "Producto";
          worksheet.Cell(currentRow, 2).Value = "Cantidad";
          worksheet.Cell(currentRow, 3).Value = "Precio Unitario";
          worksheet.Cell(currentRow, 4).Value = "Subtotal";
          worksheet.Range(currentRow, 1, currentRow, 4).Style.Font.Bold = true;
          currentRow++;

          // Detalles del pedido
          if (pedido.DetallesPedidos != null && pedido.DetallesPedidos.Any())
          {
            foreach (var detalle in pedido.DetallesPedidos)
            {
              worksheet.Cell(currentRow, 1).Value = detalle.Producto?.Nombre ?? "Producto no disponible";
              worksheet.Cell(currentRow, 2).Value = detalle.Cantidad;
              worksheet.Cell(currentRow, 3).Value = $"S/{detalle.Producto?.PrecioActual.ToString("0.00") ?? "0.00"}";
              worksheet.Cell(currentRow, 4).Value = $"S/{detalle.Subtotal.ToString("0.00")}";
              currentRow++;
            }
          }

          // Espacio entre pedidos
          currentRow += 2;
        }

        // Ajustar formato
        worksheet.Columns().AdjustToContents();

        using (var memoryStream = new MemoryStream())
        {
          workbook.SaveAs(memoryStream);
          return memoryStream.ToArray();
        }
      }
    }
  }
}