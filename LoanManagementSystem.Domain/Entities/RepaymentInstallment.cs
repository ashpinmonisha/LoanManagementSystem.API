using System;
using System.Collections.Generic;
using System.Text;
namespace LoanManagementSystem.Domain.Entities;

/// <summary>One row of the repayment schedule (3.3).</summary>
public sealed record RepaymentInstallment(
    int InstallmentNumber,
    DateOnly DueDate,
    decimal EmiAmount,
    decimal Principal,
    decimal Interest,
    decimal RemainingBalance);
