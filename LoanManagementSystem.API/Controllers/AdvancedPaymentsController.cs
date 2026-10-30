using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdvancedPaymentsController : ControllerBase
    {
        private readonly IAdvancedPaymentService _service;

        public AdvancedPaymentsController(
            IAdvancedPaymentService service)
        {
            _service = service;
        }

        // ========================================
        // 4.4.3 - PARTIAL PAYMENT
        // ========================================
        [Authorize(Roles = "Admin,Manager,Employee")]
        [HttpPost("partial-payment")]
        public async Task<IActionResult> AllocatePartialPayment(
            [FromBody] PartialPaymentDto dto)
        {
            try
            {
                var result = await _service
                    .AllocatePartialPaymentAsync(dto);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // ========================================
        // 4.4.4 - PAYMENT RECONCILIATION
        // ========================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("reconcile")]
        public async Task<IActionResult> ReconcilePayment(
            [FromBody] ReconcilePaymentDto dto)
        {
            try
            {
                bool reconciled = await _service
                    .ReconcilePaymentAsync(dto);

                if (!reconciled)
                {
                    return Conflict(new
                    {
                        message = "Payment is not completed."
                    });
                }

                return Ok(new
                {
                    reconciled = true,
                    message = "Payment reconciled successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // ========================================
        // 4.4.5 - LOAN CLOSURE
        // ========================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("close/{loanId:int}")]
        public async Task<IActionResult> CloseLoan(int loanId)
        {
            if (loanId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid Loan ID."
                });
            }

            try
            {
                bool closed = await _service.CloseLoanAsync(loanId);

                return Ok(new
                {
                    loanId,
                    closed,
                    message = closed
                        ? "Loan closed successfully."
                        : "Loan closure was not completed."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // ========================================
        // 4.4.1 - OVERDUE EMI MANAGEMENT
        // ========================================
        [Authorize(Roles = "Admin,Manager,Employee")]
        [HttpGet("overdue/{loanId:int}")]
        public async Task<IActionResult> GetOverdueEmis(int loanId)
        {
            if (loanId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid Loan ID."
                });
            }

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
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // ========================================
        // 4.4.2 - CREATE PENALTY
        // ========================================
        [Authorize(Roles = "Admin,Manager")]
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
                    penaltyId,
                    message = "Penalty created successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }
    }
}