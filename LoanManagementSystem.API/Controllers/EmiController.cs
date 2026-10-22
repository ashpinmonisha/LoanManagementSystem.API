using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers;

[ApiController]
[Route("api/emi")]
// Add [Authorize] here after Akash's / Moni's JWT authentication is merged.
public sealed class EmiController(IEmiCalculatorService emiService) : ControllerBase
{
    // 3.1 EMI Calculation
    [HttpPost("calculate")]
    public ActionResult<EmiResponse> CalculateEmi(LoanCalculationRequest request) =>
        Ok(emiService.CalculateEmi(request));

    // 3.2 Interest Calculation
    [HttpPost("interest")]
    public ActionResult<InterestResponse> CalculateInterest(LoanCalculationRequest request) =>
        Ok(emiService.CalculateInterest(request));

    // 3.3 Repayment Schedule
    [HttpPost("schedule")]
    public ActionResult<RepaymentScheduleResponse> GetRepaymentSchedule(LoanCalculationRequest request) =>
        Ok(emiService.GenerateRepaymentSchedule(request));

    // 3.4 Advanced Loan Calculation & Analysis
    [HttpPost("amortization")]
    public ActionResult<AmortizationAnalysisResponse> GetAmortization(LoanCalculationRequest request) =>
        Ok(emiService.AnalyzeAmortization(request));

    [HttpPost("prepayment")]
    public ActionResult<PrepaymentResponse> CalculatePrepayment(PrepaymentRequest request) =>
        Ok(emiService.CalculatePrepayment(request));

    [HttpPost("compare")]
    public ActionResult<LoanComparisonResponse> CompareLoans(LoanComparisonRequest request) =>
        Ok(emiService.CompareLoans(request));

    [HttpPost("summary")]
    public ActionResult<FinancialSummaryResponse> GetFinancialSummary(LoanCalculationRequest request) =>
        Ok(emiService.GetFinancialSummary(request));
}