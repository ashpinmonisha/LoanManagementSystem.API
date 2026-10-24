using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Interfaces
{
    public interface IPaymentReconciliationRepository
    {
        Task<Payment?> GetPaymentAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<List<Payment>> GetPaymentsAsync(
            int limit,
            CancellationToken ct = default);

        Task<int> CountReferenceAsync(
            string reference,
            CancellationToken ct = default);

        Task AddAsync(
            PaymentReconciliation reconciliation,
            CancellationToken ct = default);

        Task<List<PaymentReconciliation>> GetHistoryAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<List<PaymentReconciliation>> GetLatestMismatchesAsync(
            CancellationToken ct = default);

        Task<List<PaymentReconciliation>> GetLatestResultsAsync(
            CancellationToken ct = default);
    }
}