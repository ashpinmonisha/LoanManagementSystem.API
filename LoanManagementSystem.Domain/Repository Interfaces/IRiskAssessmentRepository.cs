using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Domain.RepositoryInterfaces
{
    public interface IRiskAssessmentRepository
    {
        Task AddAsync(RiskAssessment assessment);

        Task<RiskAssessment?> GetByLoanIdAsync(
            int loanApplicationId);

        Task SaveChangesAsync();
    }
}