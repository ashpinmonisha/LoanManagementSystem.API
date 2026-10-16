
using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerEligibilityController : ControllerBase
{
    private readonly ICustomerEligibilityService _eligibilityService;

    public CustomerEligibilityController(
        ICustomerEligibilityService eligibilityService)
    {
        _eligibilityService = eligibilityService;
    }

    // Save or update eligibility details
    [HttpPost("save")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveEligibility(
        [FromBody] CustomerEligibilityDto dto)
    {
        try
        {
            var result =
                await _eligibilityService.SaveEligibilityAsync(dto);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Get eligibility details by customer ID
    [HttpGet("{customerId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEligibility(int customerId)
    {
        try
        {
            var result =
                await _eligibilityService
                    .GetEligibilityByCustomerIdAsync(customerId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Check basic eligibility
    [HttpGet("{customerId:int}/check")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CheckEligibility(int customerId)
    {
        try
        {
            var result =
                await _eligibilityService.CheckEligibilityAsync(customerId);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
