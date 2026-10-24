using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Mapper;

public static class CalculationHistoryMapper
{
    public static CalculationHistoryResponse ToResponse(this LoanCalculationRecord record) =>
        new(record.Id,
            record.CalculationType.ToString(),
            record.LoanAmount,
            record.InterestRate,
            record.LoanTenureMonths,
            record.MonthlyEmi,
            record.TotalInterest,
            record.TotalRepayment,
            record.CalculatedAtUtc);
}