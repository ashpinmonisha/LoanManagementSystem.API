using LoanManagementSystem.Domain.Enums;

namespace LoanManagementSystem.Domain.Entities;

/// <summary>A saved result of a loan calculation.</summary>
public sealed class LoanCalculationRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public CalculationType CalculationType { get; init; }
    public decimal LoanAmount { get; init; }
    public decimal InterestRate { get; init; }
    public int LoanTenureMonths { get; init; }
    public decimal MonthlyEmi { get; init; }
    public decimal TotalInterest { get; init; }
    public decimal TotalRepayment { get; init; }
    public DateTime CalculatedAtUtc { get; init; } = DateTime.UtcNow;
}