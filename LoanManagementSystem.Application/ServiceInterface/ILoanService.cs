using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface ILoanService
    {
        Task<LoanApplication> ApplyLoanAsync(
            LoanApplicationDto dto);

        Task<List<LoanApplication>> GetAllLoansAsync();

        Task<LoanApplication?> GetLoanByIdAsync(
            int loanId);

        Task<LoanApplication?> MakeDecisionAsync(
            int loanId,
            LoanDecisionDto dto,
            int employeeId);
    }
}