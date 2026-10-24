namespace LoanManagementSystem.Application.DTOs.Reports;

public class RefundReportDto
{
    public int TotalRefunds { get; set; }
    public int PendingRefunds { get; set; }
    public int CompletedRefunds { get; set; }
    public int FailedRefunds { get; set; }
    public decimal CompletedRefundAmount { get; set; }
}