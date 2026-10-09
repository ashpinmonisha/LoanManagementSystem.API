using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        private readonly IRiskAssessmentService
            _riskService;

        public LoanController(
            ILoanService loanService,
            IRiskAssessmentService riskService)
        {
            _loanService = loanService;
            _riskService = riskService;
        }

        // 2.2 Loan Application
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLoan(
            LoanApplicationDto dto)
        {
            if (dto.LoanAmount <= 0)
            {
                return BadRequest(
                    "Loan amount must be greater than zero.");
            }

            if (dto.LoanTenure <= 0)
            {
                return BadRequest(
                    "Loan tenure must be greater than zero.");
            }

            var loan =
                await _loanService.ApplyLoanAsync(dto);

            return Ok(new
            {
                message =
                    "Loan application submitted successfully.",

                loanId =
                    loan.LoanApplicationId,

                status =
                    loan.Status
            });
        }


        // View all applications
        [HttpGet("applications")]
        public async Task<IActionResult>
            GetApplications()
        {
            var loans =
                await _loanService.GetAllLoansAsync();

            return Ok(loans);
        }


        // View one application
        [HttpGet("{loanId}")]
        public async Task<IActionResult>
            GetLoan(int loanId)
        {
            var loan =
                await _loanService
                    .GetLoanByIdAsync(loanId);

            if (loan == null)
            {
                return NotFound(
                    "Loan application not found.");
            }

            return Ok(loan);
        }


        // 2.3 Approval / Rejection
        [Authorize(Roles = "Employee")]
        [HttpPut("{loanId}/decision")]
        public async Task<IActionResult> MakeDecision(
    int loanId,
    LoanDecisionDto dto)
        {
            var employeeIdClaim = User.FindFirst("EmployeeId");

            if (employeeIdClaim == null)
                return Unauthorized("Employee ID not found in token.");

            int employeeId = int.Parse(employeeIdClaim.Value);

            var loan = await _loanService.MakeDecisionAsync(
                loanId,
                dto,
                employeeId);

            if (loan == null)
                return NotFound("Loan application not found.");

            return Ok(new
            {
                message = dto.Approved
                    ? "Loan approved successfully."
                    : "Loan rejected successfully.",
                loanId = loan.LoanApplicationId,
                status = loan.Status,
                remarks = loan.Remarks
            });
        }
        // 2.4 Risk Assessment
        [HttpPost("{loanId}/risk-assessment")]
        public async Task<IActionResult>
            RiskAssessment(
                int loanId,
                RiskAssessmentDto dto)
        {
            var result =
                await _riskService
                    .AssessRiskAsync(
                        loanId,
                        dto);

            if (result == null)
            {
                return NotFound(
                    "Loan application not found.");
            }

            return Ok(new
            {
                message =
                    "Risk assessment completed.",

                loanId =
                    result.LoanApplicationId,

                riskScore =
                    result.RiskScore,

                riskLevel =
                    result.RiskLevel,

                recommendation =
                    result.Recommendation
            });
        }
    }
}