using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Application.Dtos;

public sealed record PrincipalVsInterest(
    decimal TotalPrincipal,
    decimal TotalInterest,
    decimal PrincipalPercentage,
    decimal InterestPercentage);

public sealed record MonthlyBalanceItem(
    int InstallmentNumber,
    DateOnly DueDate,
    decimal Balance);

public sealed record AmortizationAnalysisResponse(
    PrincipalVsInterest PrincipalVsInterest,
    decimal TotalRepayment,
    decimal InterestToPrincipalRatio,
    IReadOnlyList<MonthlyBalanceItem> MonthlyBalances);