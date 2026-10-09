
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

        public EmployeeController(
            IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            EmployeeLoginDto dto)
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