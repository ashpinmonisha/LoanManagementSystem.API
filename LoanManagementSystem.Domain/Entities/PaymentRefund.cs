namespace LoanManagementSystem.Domain.Entities
{
    public class PaymentRefund
    {
        public int RefundId { get; set; }

        public int PaymentId { get; set; }
        public int CustomerId { get; set; }
        public int LoanId { get; set; }

        public decimal Amount { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string? RazorpayRefundId { get; set; }
        public string? RazorpayPaymentId { get; set; }

        public string? FailureReason { get; set; }

        public string IdempotencyKey { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? ApprovedBy { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectionReason { get; set; }
    }
}