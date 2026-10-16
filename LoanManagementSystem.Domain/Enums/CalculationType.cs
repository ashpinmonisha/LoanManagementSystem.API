using System;
using System.Collections.Generic;
using System.Text;

namespace LoanManagementSystem.Domain.Enums;

public enum CalculationType
{
    Emi = 1,
    Interest = 2,
    RepaymentSchedule = 3,
    Amortization = 4,
    Prepayment = 5,
    FinancialSummary = 6
}