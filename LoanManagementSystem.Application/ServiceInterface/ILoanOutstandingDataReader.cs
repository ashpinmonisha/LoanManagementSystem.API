using LoanManagementSystem.Application.DTOs.Payment;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface ILoanOutstandingDataReader
    {
        Task<LoanBalanceInfo?> GetLoanBalanceAsync(int loanId);

        Task<List<EmiBalanceInfo>> GetEmisAsync(int loanId);
    }
}