namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class OutstandingBalanceDto
    {
        public int LoanId { get; set; }

        public decimal RemainingPrincipal { get; set; }

        public decimal RemainingInterest { get; set; }

        public decimal PendingEmiAmount { get; set; }

        public decimal OverdueAmount { get; set; }

        public decimal PenaltyAmount { get; set; }

        public decimal TotalOutstanding { get; set; }
    }
}