using LoanManagementSystem.Application.Dtos;

namespace LoanManagementSystem.Application.ServiceInterface;

public interface IEmiCalculatorService
{
    // 3.1 - 3.3
    EmiResponse CalculateEmi(LoanCalculationRequest request);
    InterestResponse CalculateInterest(LoanCalculationRequest request);
    RepaymentScheduleResponse GenerateRepaymentSchedule(LoanCalculationRequest request);

    // 3.4 Advanced Loan Calculation & Analysis
    AmortizationAnalysisResponse AnalyzeAmortization(LoanCalculationRequest request);
    PrepaymentResponse CalculatePrepayment(PrepaymentRequest request);
    LoanComparisonResponse CompareLoans(LoanComparisonRequest request);
    FinancialSummaryResponse GetFinancialSummary(LoanCalculationRequest request);
}