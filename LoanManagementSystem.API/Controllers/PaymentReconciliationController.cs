using LoanManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    [Route("api/PaymentReconciliation")]
    public class PaymentReconciliationController : ControllerBase
    {
        private readonly IPaymentReconciliationService _service;

        public PaymentReconciliationController(
            IPaymentReconciliationService service)
        {
            _service = service;
        }

        // 4.9.1 - Verify Payment
        [HttpPost("verify/{paymentId:int}")]
        public async Task<IActionResult> Verify(
            int paymentId,
            CancellationToken ct)
        {
            try
            {
                var result = await _service.VerifyAsync(
                    paymentId, ct);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 4.9.2 - Bulk Reconciliation
        [HttpPost("bulk")]
        public async Task<IActionResult> Bulk(
            [FromQuery] int limit = 20,
            CancellationToken ct = default)
        {
            try
            {
                return Ok(await _service.BulkAsync(limit, ct));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 4.9.5 - Reconciliation History
        [HttpGet("history/{paymentId:int}")]
        public async Task<IActionResult> History(
            int paymentId,
            CancellationToken ct)
        {
            return Ok(await _service.HistoryAsync(
                paymentId, ct));
        }

        // 4.9.4 - Mismatches
        [HttpGet("mismatches")]
        public async Task<IActionResult> Mismatches(
            CancellationToken ct)
        {
            return Ok(await _service.MismatchesAsync(ct));
        }

        // 4.9.7 - Report
        [HttpGet("report")]
        public async Task<IActionResult> Report(
            CancellationToken ct)
        {
            return Ok(await _service.ReportAsync(ct));
        }
    }
}