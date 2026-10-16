using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Dtos;

public sealed record RepaymentScheduleResponse(
    decimal MonthlyEmi,
    decimal TotalInterest,
    decimal TotalRepayment,
    IReadOnlyList<RepaymentInstallment> Installments);