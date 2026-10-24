using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.Dtos;

public class LoanCalculationRequest
{
    [Range(1000, 100_000_000, ErrorMessage = "Loan amount must be between 1,000 and 100,000,000.")]
    public decimal LoanAmount { get; set; }

    [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100.")]
    public decimal InterestRate { get; set; }

    [Range(1, 480, ErrorMessage = "Loan tenure must be between 1 and 480 months.")]
    public int LoanTenureMonths { get; set; }

    /// <summary>Due date of the first EMI. Defaults to one month from today.</summary>
    public DateOnly? StartDate { get; set; }
}