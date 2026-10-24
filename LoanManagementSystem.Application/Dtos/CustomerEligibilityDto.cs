
using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.Dtos;

public class CustomerEligibilityDto
{
    [Required]
    public int CustomerId { get; set; }

    [Range(0, 100000000)]
    public decimal MonthlyIncome { get; set; }

    [Required]
    [StringLength(50)]
    public string EmploymentType { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string EmploymentStatus { get; set; } = string.Empty;

    [StringLength(150)]
    public string? EmployerName { get; set; }

    [Range(0, 100)]
    public int EmploymentExperienceYears { get; set; }

    [Range(0, 850)]
    public int CreditScore { get; set; }

    [Range(0, 50)]
    public int ExistingLoanCount { get; set; }

    [Range(0, 100000000)]
    public decimal ExistingMonthlyObligations { get; set; }

    [Range(0, 100000000)]
    public decimal ExistingLoanAmount { get; set; }

    [StringLength(500)]
    public string? FinancialProfileRemarks { get; set; }
}
