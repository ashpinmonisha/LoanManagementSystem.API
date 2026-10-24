namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class LoanBalanceInfo
    {
        public int LoanId { get; set; }

        public int CustomerId { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal RemainingPrincipal { get; set; }

        public decimal RemainingInterest { get; set; }
    }
}