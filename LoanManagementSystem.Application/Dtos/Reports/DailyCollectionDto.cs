namespace LoanManagementSystem.Application.DTOs.Reports;

public class DailyCollectionDto
{
    public DateTime Date { get; set; }
    public int PaymentCount { get; set; }
    public decimal TotalAmount { get; set; }
}