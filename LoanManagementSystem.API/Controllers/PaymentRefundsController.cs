using LoanManagementSystem.Application.DTOs.Refund;
using LoanManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/PaymentRefunds")]
    public class PaymentRefundsController : ControllerBase
    {
        private readonly IPaymentRefundService _service;

        public PaymentRefundsController(
            IPaymentRefundService service)
        {
            _service = service;
        }

        [HttpPost("request")]
        public async Task<IActionResult> Request(
            [FromBody] CreateRefundDto dto,
            CancellationToken ct)
        {
            try
            {
                return Ok(await _service.RequestAsync(dto, ct));
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

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("approve/{refundId:int}")]
        public async Task<IActionResult> Approve(
            int refundId,
            CancellationToken ct)
        {
            try
            {
                var employeeId =
                    User.FindFirst("EmployeeId")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? "Unknown";

                return Ok(await _service.ApproveAsync(
                    refundId, employeeId, ct));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("reject/{refundId:int}")]
        public async Task<IActionResult> Reject(
            int refundId,
            [FromBody] RefundApprovalDto dto,
            CancellationToken ct)
        {
            try
            {
                var employeeId =
                    User.FindFirst("EmployeeId")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? "Unknown";

                return Ok(await _service.RejectAsync(
                    refundId,
                    employeeId,
                    dto.Reason ?? "",
                    ct));
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

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("process/{refundId:int}")]
        public async Task<IActionResult> Process(
            int refundId,
            CancellationToken ct)
        {
            try
            {
                return Ok(await _service.ProcessAsync(refundId, ct));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("status/{refundId:int}")]
        public async Task<IActionResult> Status(
            int refundId,
            CancellationToken ct)
        {
            try
            {
                return Ok(await _service.RefreshStatusAsync(
                    refundId, ct));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("history/{paymentId:int}")]
        public async Task<IActionResult> History(
            int paymentId,
            CancellationToken ct)
        {
            return Ok(await _service.GetHistoryAsync(
                paymentId, ct));
        }
    }
}