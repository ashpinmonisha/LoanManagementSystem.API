namespace LoanManagementSystem.Application.DTOs.Refund
{
    public class RefundResponseDto
    {
        public int RefundId { get; set; }
        public int PaymentId { get; set; }
        public int CustomerId { get; set; }
        public int LoanId { get; set; }

        public decimal Amount { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string? RazorpayRefundId { get; set; }
        public string? FailureReason { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}