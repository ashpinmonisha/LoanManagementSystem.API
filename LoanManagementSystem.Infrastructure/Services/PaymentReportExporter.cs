using ClosedXML.Excel;
using LoanManagementSystem.Application.DTOs.Reports;
using LoanManagementSystem.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;

namespace LoanManagementSystem.Infrastructure.Services;

public class PaymentReportExporter : IPaymentReportExporter
{
    public ExportedReportDto ExportExcel(
        List<PaymentReportRowDto> rows)
    {
        using var workbook = new XLWorkbook();

        var sheet = workbook.Worksheets.Add("Payments");

        string[] headers =
        {
            "Payment ID",
            "Loan ID",
            "Customer ID",
            "Payment Date",
            "Amount",
            "Method",
            "Reference",
            "Status"
        };

        for (int i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        for (int i = 0; i < rows.Count; i++)
        {
            var p = rows[i];
            int row = i + 2;

            sheet.Cell(row, 1).Value = p.PaymentId;
            sheet.Cell(row, 2).Value = p.LoanId;
            sheet.Cell(row, 3).Value = p.CustomerId;
            sheet.Cell(row, 4).Value = p.PaymentDate;
            sheet.Cell(row, 5).Value = p.Amount;
            sheet.Cell(row, 6).Value = p.PaymentMethod;
            sheet.Cell(row, 7).Value = p.PaymentReference;
            sheet.Cell(row, 8).Value = p.PaymentStatus;
        }

        var header = sheet.Range(1, 1, 1, 8);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#1F4E78");
        header.Style.Font.FontColor = XLColor.White;

        sheet.Column(4).Style.DateFormat.Format =
            "yyyy-mm-dd hh:mm";

        sheet.Column(5).Style.NumberFormat.Format =
            "#,##0.00";

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new ExportedReportDto
        {
            Content = stream.ToArray(),
            FileName = $"PaymentReport_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx",
            ContentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }

    public ExportedReportDto ExportPdf(
        List<PaymentReportRowDto> rows)
    {
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header()
                    .Text("Loan Management - Payment Report")
                    .FontSize(18)
                    .Bold();

                page.Content().PaddingVertical(15).Column(column =>
                {
                    column.Item().Text(
                        $"Generated UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm}");

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            foreach (var name in new[]
                            {
                                "Payment", "Loan", "Customer",
                                "Date", "Amount", "Status"
                            })
                            {
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Padding(5)
                                    .Text(name)
                                    .Bold();
                            }
                        });

                        foreach (var p in rows)
                        {
                            AddCell(table, p.PaymentId.ToString());
                            AddCell(table, p.LoanId.ToString());
                            AddCell(table, p.CustomerId.ToString());
                            AddCell(table, p.PaymentDate.ToString("yyyy-MM-dd"));
                            AddCell(table, p.Amount.ToString("N2"));
                            AddCell(table, p.PaymentStatus);
                        }
                    });

                    column.Item()
                        .PaddingTop(15)
                        .Text($"Total: INR {rows.Sum(p => p.Amount):N2}")
                        .Bold();
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();

        return new ExportedReportDto
        {
            Content = pdf,
            FileName = $"PaymentReport_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf",
            ContentType = "application/pdf"
        };
    }

    private static void AddCell(
        TableDescriptor table,
        string value)
    {
        table.Cell()
            .BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5)
            .Text(value);
    }
}