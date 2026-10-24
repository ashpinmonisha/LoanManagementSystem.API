namespace LoanManagementSystem.Application.Dtos
{
    public class CreateRazorpayOrderDto
    {
        public int CustomerId { get; set; }
        public int LoanId { get; set; }
        public int EmiId { get; set; }
        public decimal Amount { get; set; }
    }

    public class RazorpayOrderResponseDto
    {
        public string KeyId { get; set; } = "";
        public string OrderId { get; set; } = "";
        public long Amount { get; set; }
        public string Currency { get; set; } = "INR";
    }

    public class VerifyRazorpayPaymentDto
    {
        public string RazorpayOrderId { get; set; } = "";
        public string RazorpayPaymentId { get; set; } = "";
        public string RazorpaySignature { get; set; } = "";
    }

    public class PaymentConfirmationDto
    {
        public int PaymentId { get; set; }
        public int CustomerId { get; set; }
        public int LoanId { get; set; }
        public int EmiId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentReference { get; set; } = "";
        public string PaymentMethod { get; set; } = "Razorpay";
        public string PaymentStatus { get; set; } = "";

        public DateTime PaymentDate { get; set; }
        // Added for service mapping
        public string Currency { get; set; } = "INR";
        public DateTime ConfirmedAt { get; set; }
    }
}