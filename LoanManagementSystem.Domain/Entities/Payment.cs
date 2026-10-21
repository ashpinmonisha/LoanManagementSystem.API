namespace LoanManagementSystem.Domain.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int CustomerId { get; set; }

        public int LoanId { get; set; }

        // EMIs are identified elsewhere in the system; store the related EmiId.
        // Make nullable because a payment may not be assigned to an EMI yet.
        public int? EmiId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string PaymentReference { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
