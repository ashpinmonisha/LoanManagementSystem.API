using LoanManagementSystem.Application.DTOs.Reconciliation;

namespace LoanManagementSystem.Application.Interfaces
{
    public interface IPaymentReconciliationService
    {
        Task<ReconciliationResponseDto> VerifyAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<List<ReconciliationResponseDto>> BulkAsync(
            int limit,
            CancellationToken ct = default);

        Task<List<ReconciliationResponseDto>> HistoryAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<List<ReconciliationResponseDto>> MismatchesAsync(
            CancellationToken ct = default);

        Task<ReconciliationReportDto> ReportAsync(
            CancellationToken ct = default);
    }
}