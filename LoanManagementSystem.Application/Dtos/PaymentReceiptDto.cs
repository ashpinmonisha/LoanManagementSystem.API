namespace LoanManagementSystem.Application.DTOs
{
    public class PaymentReceiptDto
    {
        public int Id { get; set; }

        public string ReceiptNumber { get; set; } = string.Empty;

        public int PaymentId { get; set; }

        public int CustomerId { get; set; }

        public int LoanId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string PaymentReference { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; }
    }
}