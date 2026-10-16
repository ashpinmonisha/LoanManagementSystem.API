
using System.Security.Claims;
using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KycDocumentController : ControllerBase
{
    private readonly IKycDocumentService _kycService;

    public KycDocumentController(IKycDocumentService kycService)
    {
        _kycService = kycService;
    }

    private int? GetCustomerId()
    {
        var id = User.FindFirst("CustomerId")?.Value
                 ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(id, out var customerId)
            ? customerId
            : null;
    }

    // Customer uploads KYC document metadata
    [HttpPost("upload")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UploadDocument(
        [FromBody] KycDocumentUploadDto dto)
    {
        var customerId = GetCustomerId();

        if (customerId == null)
            return Unauthorized(new
            {
                message = "Customer ID is missing from the token."
            });

        dto.CustomerId = customerId.Value;

        try
        {
            var result = await _kycService.UploadDocumentAsync(dto);
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

    // Customer can access only their own documents
    [HttpGet("my-documents")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyDocuments()
    {
        var customerId = GetCustomerId();

        if (customerId == null)
            return Unauthorized(new
            {
                message = "Customer ID is missing from the token."
            });

        try
        {
            var result = await _kycService
                .GetDocumentsByCustomerIdAsync(customerId.Value);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Admin can view all pending KYC documents
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingDocuments()
    {
        var result = await _kycService.GetPendingDocumentsAsync();
        return Ok(result);
    }

    // Admin approves or rejects a KYC document
    [HttpPut("{documentId:int}/verification")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateVerificationStatus(
        int documentId,
        [FromBody] KycVerificationStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            return BadRequest(new
            {
                message = "Verification status is required."
            });
        }

        try
        {
            var result = await _kycService.UpdateVerificationStatusAsync(
                documentId,
                dto.Status,
                dto.Remarks);

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
}
