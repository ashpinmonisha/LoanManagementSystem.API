using LoanManagementSystem.Application.DTOs.Payment;

namespace LoanManagementSystem.Application.ServiceInterface;

public interface ILoanModuleClient
{
    Task<LoanBalanceInfo?> GetLoanBalanceAsync(int loanId);

    Task<List<EmiBalanceInfo>> GetEmisAsync(int loanId);

    Task<EmiAllocationResult> AllocatePaymentAsync(
    int emiId,
    decimal amount,
    string idempotencyKey);

    Task<bool> CloseLoanAsync(int loanId);
}