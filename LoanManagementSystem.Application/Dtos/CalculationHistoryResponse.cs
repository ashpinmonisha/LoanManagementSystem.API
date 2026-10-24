using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Application.Dtos;

public sealed record CalculationHistoryResponse(
    Guid Id,
    string CalculationType,
    decimal LoanAmount,
    decimal InterestRate,
    int LoanTenureMonths,
    decimal MonthlyEmi,
    decimal TotalInterest,
    decimal TotalRepayment,
    DateTime CalculatedAtUtc);