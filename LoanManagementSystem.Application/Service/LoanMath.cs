using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Service;

/// <summary>Pure financial formulas used by <see cref="EmiCalculatorService"/>.</summary>
internal static class LoanMath
{
    public static decimal Round(decimal value, int digits = 2) =>
        Math.Round(value, digits, MidpointRounding.AwayFromZero);

    /// <summary>Converts an annual percentage (e.g. 10.5) to a monthly decimal rate.</summary>
    public static decimal MonthlyRate(decimal annualRate) => annualRate / 12m / 100m;

    /// <summary>EMI = P × r × (1 + r)^n / ((1 + r)^n − 1)</summary>
    public static decimal CalculateEmi(decimal principal, decimal annualRate, int months)
    {
        if (annualRate == 0)
            return Round(principal / months);

        var r = MonthlyRate(annualRate);
        var factor = Power(1 + r, months);

        return Round(principal * r * factor / (factor - 1));
    }

    /// <summary>
    /// Builds installments until the balance is cleared or <paramref name="maxCount"/> is reached.
    /// The final installment absorbs any rounding difference so the balance ends at exactly 0.
    /// </summary>
    public static List<RepaymentInstallment> Amortize(
        decimal openingBalance,
        decimal monthlyRate,
        decimal emi,
        DateOnly firstDueDate,
        int startNumber,
        int maxCount)
    {
        var installments = new List<RepaymentInstallment>();
        var balance = openingBalance;

        for (var i = 0; i < maxCount && balance > 0; i++)
        {
            var interest = Round(balance * monthlyRate);
            var principal = emi - interest;
            var payment = emi;

            if (principal >= balance || i == maxCount - 1)
            {
                principal = balance;
                payment = principal + interest;
            }

            balance = Round(balance - principal);

            installments.Add(new RepaymentInstallment(
                startNumber + i,
                firstDueDate.AddMonths(i),
                payment,
                principal,
                interest,
                balance));
        }

        return installments;
    }

    private static decimal Power(decimal value, int exponent)
    {
        var result = 1m;
        for (var i = 0; i < exponent; i++)
            result *= value;

        return result;
    }
}