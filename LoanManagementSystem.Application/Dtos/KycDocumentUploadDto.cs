
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.Dtos;

public class KycDocumentUploadDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public DateTime? ExpiryDate { get; set; }
}
