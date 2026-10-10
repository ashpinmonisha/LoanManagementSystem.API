
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Domain.Entities;

public class CustomerEligibilityProfile
{
    public int CustomerEligibilityProfileId { get; set; }

    public int CustomerId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EmploymentStatus { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? EmployerName { get; set; }

    public decimal MonthlyIncome { get; set; }

    public int EmploymentExperienceYears { get; set; }

    public decimal ExistingLoanAmount { get; set; }

    [MaxLength(500)]
    public string? FinancialProfileRemarks { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
}
