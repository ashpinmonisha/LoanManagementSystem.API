using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace LoanManagementSystem.Infrastructure.Services
{
    public class PaymentReceiptPdfGenerator
        : IPaymentReceiptPdfGenerator
    {
        public byte[] Generate(PaymentReceiptDto receipt)
        {
            var money = new CultureInfo("en-IN");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("LOAN MANAGEMENT SYSTEM")
                                .FontSize(20)
                                .Bold();

                            column.Item()
                                .Text("PAYMENT RECEIPT")
                                .FontSize(15)
                                .FontColor(Colors.Blue.Darken2);

                            column.Item().PaddingTop(10)
                                .LineHorizontal(1);
                        });

                    page.Content()
                        .PaddingVertical(25)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            AddRow(column, "Receipt Number",
                                receipt.ReceiptNumber);

                            AddRow(column, "Payment ID",
                                receipt.PaymentId.ToString());

                            AddRow(column, "Customer ID",
                                receipt.CustomerId.ToString());

                            AddRow(column, "Loan ID",
                                receipt.LoanId.ToString());

                            AddRow(column, "Payment Date",
                                receipt.PaymentDate
                                    .ToString("dd MMM yyyy"));

                            AddRow(column, "Payment Method",
                                receipt.PaymentMethod);

                            AddRow(column, "Transaction Reference",
                                receipt.PaymentReference);

                            AddRow(column, "Receipt Generated",
                                receipt.GeneratedAt
                                    .ToString("dd MMM yyyy HH:mm"));

                            column.Item()
                                .PaddingTop(20)
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(15)
                                .Column(amount =>
                                {
                                    amount.Item()
                                        .Text("AMOUNT PAID")
                                        .Bold();

                                    amount.Item()
                                        .Text(receipt.Amount
                                            .ToString("C", money))
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor(Colors.Green.Darken2);
                                });

                            column.Item()
                                .PaddingTop(15)
                                .Text("Payment Status: COMPLETED")
                                .Bold()
                                .FontColor(Colors.Green.Darken2);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Computer-generated payment receipt.");
                });
            }).GeneratePdf();
        }

        private static void AddRow(
            ColumnDescriptor column,
            string label,
            string value)
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(160).Text(label).Bold();

                row.RelativeItem().Text(value);
            });
        }
    }
}