namespace LoanManagementSystem.Application.DTOs.Reports;

public class CustomerPaymentReportDto
{
    public int CustomerId { get; set; }
    public int PaymentCount { get; set; }
    public decimal TotalPaid { get; set; }
}