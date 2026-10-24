namespace LoanManagementSystem.Domain.Entities
{
    public class Penalty
    {
        public int PenaltyId { get; set; }
        public int LoanId { get; set; }
        public int EmiId { get; set; }

        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }

        public int DaysOverdue { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public string Status { get; set; } = "Pending";
    }
}