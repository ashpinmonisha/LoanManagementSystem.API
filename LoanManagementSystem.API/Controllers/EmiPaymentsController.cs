using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/EmiPayments")]
    public class EmiPaymentsController : ControllerBase
    {
        private readonly IEmiPaymentService _service;

        public EmiPaymentsController(IEmiPaymentService service)
        {
            _service = service;
        }

        [HttpPost("allocate")]
        public async Task<IActionResult> Allocate(
            [FromBody] EmiPaymentDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.AllocatePaymentAsync(
                    dto, cancellationToken);

                return Ok(result);
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

        [HttpGet("outstanding/{loanId:int}")]
        public async Task<IActionResult> Outstanding(
            int loanId,
            CancellationToken cancellationToken)
        {
            if (loanId <= 0)
                return BadRequest(new
                {
                    message = "Invalid Loan ID"
                });

            var result = await _service.GetOutstandingAsync(
                loanId, cancellationToken);

            return Ok(result);
        }
    }
}