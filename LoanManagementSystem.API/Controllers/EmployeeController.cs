using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost("register")]
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] EmployeeLoginDto dto)
        {
            var employee =
                await _employeeService.LoginAsync(dto);

            if (employee == null)
            {
                return Unauthorized(
                    "Invalid employee credentials.");
            }

            return Ok(new
            {
                message = "Login successful",
                employee
            });
        }
    }
}