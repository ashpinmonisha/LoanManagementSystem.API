namespace LoanManagementSystem.Application.DTOs.Reports;

public class PaymentReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}