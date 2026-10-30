using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly IRiskAssessmentService _riskService;
        private readonly ILoanOutstandingDataReader _balanceReader;

        public LoanController(
            ILoanService loanService,
            IRiskAssessmentService riskService,
            ILoanOutstandingDataReader balanceReader)
        {
            _loanService = loanService;
            _riskService = riskService;
            _balanceReader = balanceReader;
        }

        // 2.2 - APPLY LOAN
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLoan(
            [FromBody] LoanApplicationDto dto)
        {
            if (dto.LoanAmount <= 0)
                return BadRequest(new
                {
                    message = "Loan amount must be greater than zero."
                });

            if (dto.LoanTenure <= 0)
                return BadRequest(new
                {
                    message = "Loan tenure must be greater than zero."
                });

            var loan = await _loanService.ApplyLoanAsync(dto);

            return Ok(new
            {
                message = "Loan application submitted successfully.",
                loanId = loan.LoanApplicationId,
                status = loan.Status
            });
        }

        // GET ALL LOAN APPLICATIONS
        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications()
        {
            var loans = await _loanService.GetAllLoansAsync();

            return Ok(loans);
        }

        // GET LOAN BY ID
        [HttpGet("{loanId:int}")]
        public async Task<IActionResult> GetLoan(int loanId)
        {
            var loan = await _loanService.GetLoanByIdAsync(loanId);

            if (loan == null)
                return NotFound(new
                {
                    message = "Loan application not found."
                });

            return Ok(loan);
        }

        // 2.3 - APPROVE / REJECT LOAN
        [Authorize(Roles = "Employee,Manager,Admin")]
        [HttpPut("{loanId:int}/decision")]
        public async Task<IActionResult> MakeDecision(
            int loanId,
            [FromBody] LoanDecisionDto dto)
        {
            var employeeIdClaim = User.FindFirst("EmployeeId");

            if (!int.TryParse(
                    employeeIdClaim?.Value,
                    out int employeeId))
            {
                return Forbid();
            }

            var loan = await _loanService.MakeDecisionAsync(
                loanId,
                dto,
                employeeId);

            if (loan == null)
                return NotFound(new
                {
                    message = "Loan application not found."
                });

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

        // 2.4 - RISK ASSESSMENT
        [HttpPost("{loanId:int}/risk-assessment")]
        public async Task<IActionResult> RiskAssessment(
            int loanId,
            [FromBody] RiskAssessmentDto dto)
        {
            var result = await _riskService.AssessRiskAsync(
                loanId,
                dto);

            if (result == null)
                return NotFound(new
                {
                    message = "Loan application not found."
                });

            return Ok(new
            {
                message = "Risk assessment completed.",
                loanId = result.LoanApplicationId,
                riskScore = result.RiskScore,
                riskLevel = result.RiskLevel,
                recommendation = result.Recommendation
            });
        }

        // MODULE 4.3 - GET LOAN BALANCE
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{loanId:int}/balance")]
        public async Task<IActionResult> GetLoanBalance(int loanId)
        {
            if (loanId <= 0)
                return BadRequest(new
                {
                    message = "Invalid Loan ID."
                });

            try
            {
                var balance = await _balanceReader
                    .GetLoanBalanceAsync(loanId);

                if (balance == null)
                    return NotFound(new
                    {
                        message = "Loan not found.",
                        loanId
                    });

                return Ok(balance);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message,
                    loanId
                });
            }
        }
    }
}