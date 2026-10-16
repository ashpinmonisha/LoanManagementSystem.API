using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Application.Dtos;

public sealed record InterestResponse(
    decimal PrincipalAmount,
    decimal MonthlyInterest,   // interest charged in the first month
    decimal TotalInterest,
    decimal TotalRepayment);
