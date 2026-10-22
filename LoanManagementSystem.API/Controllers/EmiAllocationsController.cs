using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/EmiAllocations")]
public class EmiAllocationsController : ControllerBase
{
    private readonly IEmiAllocationService _service;

    public EmiAllocationsController(
        IEmiAllocationService service)
    {
        _service = service;
    }

    [HttpPost("allocate")]
    public async Task<IActionResult> Allocate(
        [FromBody] AllocateEmiPaymentDto dto,
        CancellationToken ct)
    {
        try
        {
            var result = await _service.AllocateAsync(dto, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("outstanding/{loanId:int}")]
    public async Task<IActionResult> Outstanding(
        int loanId,
        CancellationToken ct)
    {
        try
        {
            return Ok(
                await _service.GetOutstandingAsync(loanId, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}