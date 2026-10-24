using System.Security.Claims;
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
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> SaveEligibility(
        [FromBody] CustomerEligibilityDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!TryGetAuthorizedCustomerId(dto.CustomerId, out var customerId))
            return Forbid();

        dto.CustomerId = customerId;

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
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetEligibility(int customerId)
    {
        if (!TryGetAuthorizedCustomerId(customerId, out _))
            return Forbid();

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
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> CheckEligibility(int customerId)
    {
        if (!TryGetAuthorizedCustomerId(customerId, out _))
            return Forbid();

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

    private bool TryGetAuthorizedCustomerId(int requestedCustomerId, out int customerId)
    {
        customerId = requestedCustomerId;

        if (User.IsInRole("Admin"))
            return requestedCustomerId > 0;

        var claim = User.FindFirst("CustomerId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(claim, out var tokenCustomerId) &&
               tokenCustomerId == requestedCustomerId;
    }
}
