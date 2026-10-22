namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }

        public int CustomerId { get; set; }

        public int LoanId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string PaymentReference { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;
    }
}