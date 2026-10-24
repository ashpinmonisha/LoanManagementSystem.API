namespace LoanManagementSystem.Application.DTOs
{
    public class EmiPaymentDto
    {
        public int PaymentId { get; set; }
        public int EmiId { get; set; }
    }

    public class EmiDetailsDto
    {
        public int EmiId { get; set; }
        public int LoanId { get; set; }

        public decimal EmiAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingAmount { get; set; }

        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "";
    }

    public class EmiPaymentResultDto
    {
        public int AllocationId { get; set; }
        public int PaymentId { get; set; }
        public int EmiId { get; set; }

        public decimal AllocatedAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public string AllocationStatus { get; set; } = "";
        public string EmiStatus { get; set; } = "";
    }

    public class EmiOutstandingDto
    {
        public int LoanId { get; set; }

        public decimal TotalEmiAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal OverdueAmount { get; set; }

        public List<EmiDetailsDto> Emis { get; set; } = new();
    }

    public class ApplyEmiPaymentDto
    {
        public int PaymentId { get; set; }
        public int EmiId { get; set; }
        public decimal Amount { get; set; }
        public string IdempotencyKey { get; set; } = "";
    }
}
