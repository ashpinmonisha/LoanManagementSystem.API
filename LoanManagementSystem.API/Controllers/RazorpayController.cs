using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RazorpayController : ControllerBase
    {
        private readonly IRazorpayPaymentService _service;

        public RazorpayController(
            IRazorpayPaymentService service)
        {
            _service = service;
        }

        // POST: api/Razorpay/create-order
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateRazorpayOrderDto dto)
        {
            try
            {
                var result =
                    await _service.CreateOrderAsync(dto);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // POST: api/Razorpay/verify
        [HttpPost("verify")]
        public async Task<IActionResult> Verify(
            [FromBody] VerifyRazorpayPaymentDto dto)
        {
            var result =
                await _service.VerifyPaymentAsync(dto);

            if (result == null)
            {
                return BadRequest(new
                {
                    message =
                        "Payment is not verified or captured."
                });
            }

            return Ok(new
            {
                message = "Payment confirmed successfully",
                payment = result
            });
        }

        // GET: api/Razorpay/confirmation/{orderId}
        [HttpGet("confirmation/{orderId}")]
        public async Task<IActionResult> GetConfirmation(
            string orderId)
        {
            var result =
                await _service.GetConfirmationAsync(orderId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Confirmed payment not found."
                });
            }

            return Ok(result);
        }
    }
}