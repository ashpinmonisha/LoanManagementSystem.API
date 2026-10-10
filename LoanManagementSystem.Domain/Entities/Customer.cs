
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    // Store only a password hash, never the plain password.
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<KycDocument> KycDocuments { get; set; }
        = new List<KycDocument>();

    public CustomerEligibilityProfile? EligibilityProfile { get; set; }
}
