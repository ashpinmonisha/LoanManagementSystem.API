
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Domain.Entities;

public class KycDocument
{
    public int KycDocumentId { get; set; }

    public int CustomerId { get; set; }

    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DocumentNumber { get; set; }

    // Store the file path or storage key, not the file bytes.
    [MaxLength(500)]
    public string? FilePath { get; set; }

    [Required]
    [MaxLength(20)]
    public string VerificationStatus { get; set; } = "Pending";

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public Customer Customer { get; set; } = null!;
}
