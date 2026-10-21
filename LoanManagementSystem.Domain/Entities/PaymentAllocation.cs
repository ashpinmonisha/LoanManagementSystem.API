namespace LoanManagementSystem.Domain.Entities
{
    public class PaymentAllocation
    {
        public int Id { get; set; }

        public int PaymentId { get; set; }
        public int CustomerId { get; set; }
        public int LoanId { get; set; }
        public int EmiId { get; set; }

        public decimal Amount { get; set; }

        public string IdempotencyKey { get; set; } = "";

        // Pending, Applied, Rejected
        public string Status { get; set; } = "Pending";

        public string? LastError { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AppliedAt { get; set; }
    }
}