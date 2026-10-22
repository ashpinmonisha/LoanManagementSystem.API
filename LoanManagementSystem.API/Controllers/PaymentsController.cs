using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Mvc;
namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOutstandingBalanceService _outstandingService;

        // ONLY ONE CONSTRUCTOR
        public PaymentsController(
            IPaymentService paymentService,
            IOutstandingBalanceService outstandingService)
        {
            _paymentService = paymentService;
            _outstandingService = outstandingService;
        }

        // 4.1 RECORD PAYMENT
        [HttpPost]
        public async Task<IActionResult> RecordPayment(
            [FromBody] RecordPaymentDto dto)
        {
            try
            {
                var result = await _paymentService
                    .RecordPaymentAsync(dto);

                return StatusCode(201, result);
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

        // 4.2 PAYMENT HISTORY
        [HttpGet("history/{loanId:int}")]
        public async Task<IActionResult> GetPaymentHistory(int loanId)
        {
            if (loanId <= 0)
                return BadRequest("Invalid Loan ID");

            var result = await _paymentService
                .GetPaymentHistoryAsync(loanId);

            return Ok(result);
        }

        // 4.2 CUSTOMER PAYMENT HISTORY
        [HttpGet("history/{loanId:int}/{customerId:int}")]
        public async Task<IActionResult> GetCustomerPaymentHistory(
            int loanId,
            int customerId)
        {
            if (loanId <= 0 || customerId <= 0)
                return BadRequest("Invalid Loan ID or Customer ID");

            var result = await _paymentService
                .GetCustomerPaymentHistoryAsync(
                    loanId, customerId);

            return Ok(result);
        }

        // 4.3 OUTSTANDING BALANCE
        [HttpGet("outstanding/{loanId:int}")]
        public async Task<IActionResult> GetOutstandingBalance(int loanId)
        {
            if (loanId <= 0)
                return BadRequest("Invalid Loan ID");

            try
            {
                var result = await _outstandingService
                    .GetOutstandingBalanceAsync(loanId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}