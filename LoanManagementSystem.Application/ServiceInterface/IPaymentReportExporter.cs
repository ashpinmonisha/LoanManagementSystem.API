using LoanManagementSystem.Application.DTOs.Reports;

namespace LoanManagementSystem.Application.Interfaces;

public interface IPaymentReportExporter
{
    ExportedReportDto ExportExcel(
        List<PaymentReportRowDto> rows);

    ExportedReportDto ExportPdf(
        List<PaymentReportRowDto> rows);
}