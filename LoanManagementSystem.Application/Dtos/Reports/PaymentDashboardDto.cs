namespace LoanManagementSystem.Application.DTOs.Reports;

public class PaymentDashboardDto
{
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int PendingPayments { get; set; }

    public decimal GrossCollections { get; set; }
    public decimal CompletedRefunds { get; set; }
    public decimal NetCollections { get; set; }
}