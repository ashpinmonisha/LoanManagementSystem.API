using LoanManagementSystem.Application.DTOs.Payment;

namespace LoanManagementSystem.Application.ServiceInterface;

public interface IAdvancedPaymentService
{
    Task<List<OverdueEmiDto>> GetOverdueEmisAsync(
        int loanId);

    Task<int> CreatePenaltyAsync(
        CreatePenaltyDto dto);

    Task<EmiAllocationResult> AllocatePartialPaymentAsync(
        PartialPaymentDto dto);

    Task<bool> ReconcilePaymentAsync(
        ReconcilePaymentDto dto);

    Task<bool> CloseLoanAsync(int loanId);
}