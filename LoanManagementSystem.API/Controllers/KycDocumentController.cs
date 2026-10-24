
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
    private readonly IWebHostEnvironment _environment;

    public KycDocumentController(
        IKycDocumentService kycService,
        IWebHostEnvironment environment)
    {
        _kycService = kycService;
        _environment = environment;
    }

    private int? GetCustomerId()
    {
        var id = User.FindFirst("CustomerId")?.Value
                 ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(id, out var customerId)
            ? customerId
            : null;
    }

    // Customer uploads a KYC document.
    [HttpPost("upload")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UploadDocument(
        [FromForm] string documentType,
        [FromForm] string documentNumber,
        [FromForm] DateTime? expiryDate,
        [FromForm] IFormFile file)
    {
        var customerId = GetCustomerId();

        if (customerId == null)
            return Unauthorized(new
            {
                message = "Customer ID is missing from the token."
            });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "A document file is required." });

        const long maxFileSize = 5 * 1024 * 1024;
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (file.Length > maxFileSize || !allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message = "Only PDF, JPG, JPEG, and PNG files up to 5 MB are allowed."
            });
        }

        var storageDirectory = Path.Combine(
            _environment.ContentRootPath, "Storage", "Kyc");
        Directory.CreateDirectory(storageDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var storedPath = Path.Combine(storageDirectory, storedFileName);

        await using (var stream = System.IO.File.Create(storedPath))
        {
            await file.CopyToAsync(stream);
        }

        var dto = new KycDocumentUploadDto
        {
            CustomerId = customerId.Value,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            ExpiryDate = expiryDate,
            FilePath = Path.Combine("Storage", "Kyc", storedFileName)
        };

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
            System.IO.File.Delete(storedPath);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{documentId:int}/file")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DownloadDocument(int documentId)
    {
        var customerId = GetCustomerId();
        if (customerId == null)
            return Unauthorized();

        if (!await _kycService.CustomerOwnsDocumentAsync(documentId, customerId.Value))
            return Forbid();

        var relativePath = await _kycService.GetDocumentFilePathAsync(documentId);
        var fullPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, relativePath));
        var storageRoot = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "Storage", "Kyc"));

        if (!fullPath.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase) ||
            !System.IO.File.Exists(fullPath))
            return NotFound();

        return PhysicalFile(fullPath, "application/octet-stream");
    }

    [HttpGet("{documentId:int}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetDocument(int documentId)
    {
        if (User.IsInRole("Customer"))
        {
            var customerId = GetCustomerId();
            if (customerId == null)
                return Unauthorized();

            if (!await _kycService.CustomerOwnsDocumentAsync(documentId, customerId.Value))
                return Forbid();
        }

        try
        {
            return Ok(await _kycService.GetDocumentByIdAsync(documentId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
