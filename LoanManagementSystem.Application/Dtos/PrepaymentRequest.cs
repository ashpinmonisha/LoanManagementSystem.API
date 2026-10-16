using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.Dtos;

public sealed class PrepaymentRequest : LoanCalculationRequest
{
    /// <summary>Number of EMIs already paid (0 = prepay before the first EMI).</summary>
    [Range(0, 479, ErrorMessage = "Installments paid must be between 0 and 479.")]
    public int InstallmentsPaid { get; set; }

    [Range(1, 100_000_000, ErrorMessage = "Prepayment amount must be greater than 0.")]
    public decimal PrepaymentAmount { get; set; }
}