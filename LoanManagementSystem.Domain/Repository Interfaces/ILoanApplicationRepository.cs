using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Domain.RepositoryInterfaces
{
    public interface ILoanApplicationRepository
    {
        Task<LoanApplication?> GetByIdAsync(
            int loanApplicationId);

        Task<List<LoanApplication>> GetAllAsync();

        Task AddAsync(LoanApplication loanApplication);

        Task SaveChangesAsync();
    }
}