
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.Dtos;

public class CustomerLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
