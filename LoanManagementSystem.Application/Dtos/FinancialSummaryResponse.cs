using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Application.Dtos;

public sealed record EmiSummary(
    decimal MonthlyEmi,
    int LoanTenureMonths,
    int NumberOfInstallments,
    decimal AverageMonthlyInterest);

public sealed record LoanCostAnalysis(
    decimal InterestToPrincipalRatio,
    decimal TotalCostPercentage,          // total repayment as % of the loan amount
    decimal InterestPerHundredBorrowed);

public sealed record FinancialSummaryResponse(
    decimal TotalPrincipal,
    decimal TotalInterest,
    decimal TotalRepayment,
    EmiSummary EmiSummary,
    LoanCostAnalysis LoanCostAnalysis);