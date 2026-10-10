
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.Dtos;

public class KycVerificationStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Remarks { get; set; }
}
