namespace LoanManagementSystem.Application.DTOs.Reports;

public class PaymentReportRowDto
{
    public int PaymentId { get; set; }
    public int LoanId { get; set; }
    public int CustomerId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "";
    public string PaymentReference { get; set; } = "";
    public string PaymentStatus { get; set; } = "";
}