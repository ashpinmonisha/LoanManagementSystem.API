using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.Dtos;

public sealed class LoanComparisonRequest
{
    [Required]
    [MinLength(2, ErrorMessage = "Provide at least 2 loan options.")]
    [MaxLength(5, ErrorMessage = "A maximum of 5 loan options can be compared.")]
    public List<LoanCalculationRequest> Options { get; set; } = [];
}