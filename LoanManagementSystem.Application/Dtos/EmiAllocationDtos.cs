namespace LoanManagementSystem.Application.DTOs;

public class AllocateEmiPaymentDto
{
    public int PaymentId { get; set; }
    public int EmiId { get; set; }
}

public class EmiAllocationResultDto
{
    public int AllocationId { get; set; }
    public int PaymentId { get; set; }
    public int EmiId { get; set; }

    public decimal AllocatedAmount { get; set; }

    public string AllocationStatus { get; set; } = "";
    public string EmiStatus { get; set; } = "";

    public decimal RemainingEmiAmount { get; set; }
}

public class ApplyEmiPaymentRequestDto
{
    public int PaymentId { get; set; }
    public int EmiId { get; set; }
    public decimal Amount { get; set; }
    public string IdempotencyKey { get; set; } = "";
}

public class ApplyEmiPaymentResponseDto
{
    public int EmiId { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = "";
}