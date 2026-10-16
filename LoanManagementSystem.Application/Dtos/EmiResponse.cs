using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Application.Dtos;

public sealed record EmiResponse(
    decimal LoanAmount,
    decimal InterestRate,
    int LoanTenureMonths,
    decimal MonthlyEmi);