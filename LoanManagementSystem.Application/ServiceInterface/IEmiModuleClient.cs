using LoanManagementSystem.Application.DTOs;

namespace LoanManagementSystem.Application.Interfaces;

public interface IEmiModuleClient
{
    Task<EmiDetailsDto?> GetEmiAsync(
        int emiId,
        CancellationToken ct = default);

    Task<ApplyEmiPaymentResponseDto?> ApplyPaymentAsync(
        ApplyEmiPaymentRequestDto request,
        CancellationToken ct = default);

    Task<ApplyEmiPaymentResponseDto?> GetAllocationAsync(
        string idempotencyKey,
        CancellationToken ct = default);

    Task<EmiOutstandingDto> GetOutstandingAsync(
        int loanId,
        CancellationToken ct = default);
}