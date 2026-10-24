using LoanManagementSystem.Application.DTOs.Payment;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface IOutstandingBalanceService
    {
        Task<OutstandingBalanceDto> GetOutstandingBalanceAsync(
            int loanId);
    }
}