
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.Dtos;

public class CustomerProfileUpdateDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20, MinimumLength = 7)]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }
}
