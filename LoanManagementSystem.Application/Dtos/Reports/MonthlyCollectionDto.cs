namespace LoanManagementSystem.Application.DTOs.Reports;

public class MonthlyCollectionDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int PaymentCount { get; set; }
    public decimal TotalAmount { get; set; }
}