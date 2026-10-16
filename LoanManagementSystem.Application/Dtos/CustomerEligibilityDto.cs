
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

    [Range(0, 850)]
    public int CreditScore { get; set; }

    [Range(0, 50)]
    public int ExistingLoanCount { get; set; }

    [Range(0, 100000000)]
    public decimal ExistingMonthlyObligations { get; set; }
}
