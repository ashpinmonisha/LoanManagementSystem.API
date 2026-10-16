using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Dtos;

public sealed record PrepaymentResponse(
    decimal PrepaymentAmount,
    decimal OutstandingBeforePrepayment,
    decimal NewOutstanding,
    decimal InterestSaved,
    int OriginalTenureMonths,
    int RevisedTenureMonths,
    int MonthsSaved,
    decimal OriginalTotalInterest,
    decimal RevisedTotalInterest,
    IReadOnlyList<RepaymentInstallment> RevisedSchedule);