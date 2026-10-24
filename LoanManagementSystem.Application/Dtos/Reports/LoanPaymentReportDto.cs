namespace LoanManagementSystem.Application.DTOs.Reports;

public class LoanPaymentReportDto
{
    public int LoanId { get; set; }
    public int PaymentCount { get; set; }
    public decimal TotalPaid { get; set; }
}