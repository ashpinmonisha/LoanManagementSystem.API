using LoanManagementSystem.Application.DTOs;

namespace LoanManagementSystem.Application.Interfaces;

public interface IEmiAllocationService
{
    Task<EmiAllocationResultDto> AllocateAsync(
        AllocateEmiPaymentDto dto,
        CancellationToken ct = default);

    Task<EmiOutstandingDto> GetOutstandingAsync(
        int loanId,
        CancellationToken ct = default);
}