
using System.Security.Claims;
using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // POST: api/customer/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] CustomerRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerService.RegisterAsync(dto);

            if (result == null)
                return BadRequest(new
                {
                    message = "Customer registration failed."
                });

            return Ok(result);
        }

        // POST: api/customer/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] CustomerLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _customerService.LoginAsync(dto);

            if (result == null)
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });

            return Ok(result);
        }

        // GET: api/customer/profile
        [HttpGet("profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetProfile()
        {
            var customerIdClaim =
                User.FindFirst("CustomerId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new
                {
                    message = "Invalid customer token."
                });

            var result =
                await _customerService.GetProfileAsync(customerId);

            if (result == null)
                return NotFound(new
                {
                    message = "Customer profile not found."
                });

            return Ok(result);
        }

        // PUT: api/customer/profile
        [HttpPut("profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] CustomerProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customerIdClaim =
                User.FindFirst("CustomerId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new
                {
                    message = "Invalid customer token."
                });

            var result =
                await _customerService.UpdateProfileAsync(customerId, dto);

            if (result == null)
                return NotFound(new
                {
                    message = "Customer not found or profile update failed."
                });

            return Ok(result);
        }
    }
}
