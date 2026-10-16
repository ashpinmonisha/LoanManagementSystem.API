using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvancedPaymentsController : ControllerBase
    {
        private readonly IAdvancedPaymentService _service;

        public AdvancedPaymentsController(
            IAdvancedPaymentService service)
        {
            _service = service;
        }

        // 4.4.1 Overdue Management
        [HttpGet("overdue/{loanId:int}")]
        public async Task<IActionResult> GetOverdueEmis(int loanId)
        {
            if (loanId <= 0)
                return BadRequest("Invalid Loan ID");

            try
            {
                var result = await _service
                    .GetOverdueEmisAsync(loanId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 4.4.2 Penalty
        [HttpPost("penalty")]
        public async Task<IActionResult> CreatePenalty(
            [FromBody] CreatePenaltyDto dto)
        {
            try
            {
                int penaltyId = await _service
                    .CreatePenaltyAsync(dto);

                return StatusCode(201, new
                {
                    PenaltyId = penaltyId,
                    Message = "Penalty created successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}