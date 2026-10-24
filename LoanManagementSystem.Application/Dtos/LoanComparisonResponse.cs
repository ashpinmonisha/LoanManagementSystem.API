using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Application.Dtos;

public sealed record LoanOptionResult(
    string Label,
    decimal LoanAmount,
    decimal InterestRate,
    int LoanTenureMonths,
    decimal MonthlyEmi,
    decimal TotalInterest,
    decimal TotalRepayment);

public sealed record LoanComparisonResponse(
    IReadOnlyList<LoanOptionResult> Options,
    string LowestInterestRateOption,
    string ShortestTenureOption,
    string LowestEmiOption,
    string LowestTotalInterestOption);
