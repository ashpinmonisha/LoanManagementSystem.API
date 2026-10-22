using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.Enums;
using LoanManagementSystem.Domain.RepositoryInterfaces;

namespace LoanManagementSystem.Application.Service;

public sealed class EmiCalculatorService(ILoanCalculationRepository repository) : IEmiCalculatorService
{
    private const int MaxSimulationMonths = 1200;

    // ===================== 3.1 EMI Calculation =====================
    public EmiResponse CalculateEmi(LoanCalculationRequest request)
    {
        Track(CalculationType.Emi, request, BuildSchedule(request));

        return new EmiResponse(
            request.LoanAmount,
            request.InterestRate,
            request.LoanTenureMonths,
            GetEmi(request));
    }

    // ===================== 3.2 Interest Calculation =====================
    public InterestResponse CalculateInterest(LoanCalculationRequest request)
    {
        var schedule = BuildSchedule(request);
        Track(CalculationType.Interest, request, schedule);

        return new InterestResponse(
            request.LoanAmount,
            schedule[0].Interest,
            schedule.Sum(x => x.Interest),
            schedule.Sum(x => x.EmiAmount));
    }

    // ===================== 3.3 Repayment Schedule =====================
    public RepaymentScheduleResponse GenerateRepaymentSchedule(LoanCalculationRequest request)
    {
        var schedule = BuildSchedule(request);
        Track(CalculationType.RepaymentSchedule, request, schedule);

        return new RepaymentScheduleResponse(
            GetEmi(request),
            schedule.Sum(x => x.Interest),
            schedule.Sum(x => x.EmiAmount),
            schedule);
    }

    // ===================== 3.4 Amortization Analysis =====================
    public AmortizationAnalysisResponse AnalyzeAmortization(LoanCalculationRequest request)
    {
        var schedule = BuildSchedule(request);
        Track(CalculationType.Amortization, request, schedule);

        var totalPrincipal = schedule.Sum(x => x.Principal);
        var totalInterest = schedule.Sum(x => x.Interest);
        var totalRepayment = totalPrincipal + totalInterest;

        var split = new PrincipalVsInterest(
            totalPrincipal,
            totalInterest,
            LoanMath.Round(totalPrincipal / totalRepayment * 100),
            LoanMath.Round(totalInterest / totalRepayment * 100));

        var balances = schedule
            .Select(x => new MonthlyBalanceItem(x.InstallmentNumber, x.DueDate, x.RemainingBalance))
            .ToList();

        return new AmortizationAnalysisResponse(
            split,
            totalRepayment,
            LoanMath.Round(totalInterest / totalPrincipal, 4),
            balances);
    }

    // ===================== 3.4 Prepayment Calculation =====================
    // The EMI stays the same and the tenure reduces.
    public PrepaymentResponse CalculatePrepayment(PrepaymentRequest request)
    {
        if (request.InstallmentsPaid >= request.LoanTenureMonths)
            throw new BusinessRuleException("Installments paid must be less than the loan tenure.");

        var original = BuildSchedule(request);
        var paid = request.InstallmentsPaid;

        var outstanding = paid == 0
            ? request.LoanAmount
            : original[paid - 1].RemainingBalance;

        if (request.PrepaymentAmount >= outstanding)
            throw new BusinessRuleException(
                $"Prepayment must be less than the outstanding balance ({outstanding:N2}). Use loan closure instead.");

        var newOutstanding = outstanding - request.PrepaymentAmount;

        var revised = LoanMath.Amortize(
            newOutstanding,
            LoanMath.MonthlyRate(request.InterestRate),
            GetEmi(request),
            original[paid].DueDate,
            paid + 1,
            MaxSimulationMonths);

        var originalInterest = original.Sum(x => x.Interest);
        var revisedInterest = original.Take(paid).Sum(x => x.Interest) + revised.Sum(x => x.Interest);
        var revisedTenure = paid + revised.Count;

        Track(CalculationType.Prepayment, request, original);

        return new PrepaymentResponse(
            request.PrepaymentAmount,
            outstanding,
            newOutstanding,
            originalInterest - revisedInterest,
            request.LoanTenureMonths,
            revisedTenure,
            request.LoanTenureMonths - revisedTenure,
            originalInterest,
            revisedInterest,
            revised);
    }

    // ===================== 3.4 Loan Comparison =====================
    public LoanComparisonResponse CompareLoans(LoanComparisonRequest request)
    {
        var results = request.Options
            .Select((option, index) =>
            {
                var schedule = BuildSchedule(option);

                return new LoanOptionResult(
                    $"Option {index + 1}",
                    option.LoanAmount,
                    option.InterestRate,
                    option.LoanTenureMonths,
                    GetEmi(option),
                    schedule.Sum(x => x.Interest),
                    schedule.Sum(x => x.EmiAmount));
            })
            .ToList();

        return new LoanComparisonResponse(
            results,
            results.MinBy(x => x.InterestRate)!.Label,
            results.MinBy(x => x.LoanTenureMonths)!.Label,
            results.MinBy(x => x.MonthlyEmi)!.Label,
            results.MinBy(x => x.TotalInterest)!.Label);
    }

    // ===================== 3.4 Financial Summary =====================
    public FinancialSummaryResponse GetFinancialSummary(LoanCalculationRequest request)
    {
        var schedule = BuildSchedule(request);
        Track(CalculationType.FinancialSummary, request, schedule);

        var totalInterest = schedule.Sum(x => x.Interest);
        var totalRepayment = schedule.Sum(x => x.EmiAmount);

        var emiSummary = new EmiSummary(
            GetEmi(request),
            request.LoanTenureMonths,
            schedule.Count,
            LoanMath.Round(totalInterest / schedule.Count));

        var costAnalysis = new LoanCostAnalysis(
            LoanMath.Round(totalInterest / request.LoanAmount, 4),
            LoanMath.Round(totalRepayment / request.LoanAmount * 100),
            LoanMath.Round(totalInterest / request.LoanAmount * 100));

        return new FinancialSummaryResponse(
            request.LoanAmount,
            totalInterest,
            totalRepayment,
            emiSummary,
            costAnalysis);
    }

    // ===================== private helpers =====================
    private static decimal GetEmi(LoanCalculationRequest request) =>
        LoanMath.CalculateEmi(request.LoanAmount, request.InterestRate, request.LoanTenureMonths);

    private static List<RepaymentInstallment> BuildSchedule(LoanCalculationRequest request)
    {
        var firstDueDate = request.StartDate ?? DateOnly.FromDateTime(DateTime.Today).AddMonths(1);

        return LoanMath.Amortize(
            request.LoanAmount,
            LoanMath.MonthlyRate(request.InterestRate),
            GetEmi(request),
            firstDueDate,
            startNumber: 1,
            maxCount: request.LoanTenureMonths);
    }

    /// <summary>Saves the calculation so it appears in the history endpoints.</summary>
    private void Track(CalculationType type, LoanCalculationRequest request, List<RepaymentInstallment> schedule) =>
        repository.Add(new LoanCalculationRecord
        {
            CalculationType = type,
            LoanAmount = request.LoanAmount,
            InterestRate = request.InterestRate,
            LoanTenureMonths = request.LoanTenureMonths,
            MonthlyEmi = GetEmi(request),
            TotalInterest = schedule.Sum(x => x.Interest),
            TotalRepayment = schedule.Sum(x => x.EmiAmount)
        });
}