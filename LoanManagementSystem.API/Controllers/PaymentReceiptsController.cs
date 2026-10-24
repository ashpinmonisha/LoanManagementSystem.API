using LoanManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/PaymentReceipts")]
    public class PaymentReceiptsController : ControllerBase
    {
        private readonly IPaymentReceiptService _service;
        private readonly IPaymentReceiptPdfGenerator _pdfGenerator;

        public PaymentReceiptsController(
            IPaymentReceiptService service,
            IPaymentReceiptPdfGenerator pdfGenerator)
        {
            _service = service;
            _pdfGenerator = pdfGenerator;
        }

        [HttpPost("generate/{paymentId:int}")]
        public async Task<IActionResult> Generate(
            int paymentId,
            CancellationToken ct)
        {
            try
            {
                var result = await _service.GenerateAsync(
                    paymentId, ct);

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

        [HttpGet("{paymentId:int}")]
        public async Task<IActionResult> Get(
            int paymentId,
            CancellationToken ct)
        {
            var result = await _service.GetAsync(paymentId, ct);

            if (result == null)
                return NotFound(new
                {
                    message = "Receipt not found."
                });

            return Ok(result);
        }

        [HttpGet("loan/{loanId:int}")]
        public async Task<IActionResult> GetByLoan(
            int loanId,
            CancellationToken ct)
        {
            try
            {
                var result = await _service.GetByLoanAsync(
                    loanId, ct);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("download/{paymentId:int}")]
        public async Task<IActionResult> Download(
            int paymentId,
            CancellationToken ct)
        {
            var receipt = await _service.GetAsync(
                paymentId, ct);

            if (receipt == null)
                return NotFound(new
                {
                    message = "Generate the receipt first."
                });

            var pdf = _pdfGenerator.Generate(receipt);

            return File(
                pdf,
                "application/pdf",
                $"Receipt-{receipt.PaymentId}.pdf");
        }
    }
}