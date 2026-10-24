namespace LoanManagementSystem.Domain.Entities
{
    public class RazorpayPaymentOrder
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public int LoanId { get; set; }
        public int EmiId { get; set; }

        public decimal Amount { get; set; }

        public string RazorpayOrderId { get; set; } = "";
        public string? RazorpayPaymentId { get; set; }

        // Created / Completed
        public string Status { get; set; } = "Created";

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? VerifiedAt { get; set; }
    }
}