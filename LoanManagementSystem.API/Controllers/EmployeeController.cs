using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(
            IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // REGISTER EMPLOYEE
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(
            [FromBody] CreateEmployeeDto dto)
        {
            try
            {
                int employeeId =
                    await _employeeService.CreateEmployeeAsync(dto);

                return StatusCode(201, new
                {
                    message = "Employee created successfully",
                    employeeId
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // EMPLOYEE LOGIN
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] EmployeeLoginDto dto)
        {
            var employee =
                await _employeeService.LoginAsync(dto);

            if (employee == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid employee credentials."
                });
            }

            return Ok(new
            {
                message = "Login successful",
                employee
            });
        }

        // RESET EMPLOYEE PASSWORD
        [HttpPut("reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetEmployeePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.EmployeeCode) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                dto.NewPassword.Length < 12)
            {
                return BadRequest(new
                {
                    message =
                        "Employee code and a password of at least 12 characters are required."
                });
            }

            bool success =
                await _employeeService.ResetPasswordAsync(
                    dto.EmployeeCode,
                    dto.NewPassword);

            if (!success)
            {
                return NotFound(new
                {
                    message = "Employee not found."
                });
            }

            return Ok(new
            {
                message = "Employee password reset successfully."
            });
        }
    }
}