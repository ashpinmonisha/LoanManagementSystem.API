using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface IRiskAssessmentService
    {
        Task<RiskAssessment?> AssessRiskAsync(
            int loanId,
            RiskAssessmentDto dto);
    }
}