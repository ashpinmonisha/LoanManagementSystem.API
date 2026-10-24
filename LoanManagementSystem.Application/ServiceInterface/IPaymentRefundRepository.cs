using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Interfaces
{
    public interface IPaymentRefundRepository
    {
        Task<Payment?> GetPaymentAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<PaymentRefund?> GetByIdAsync(
            int refundId,
            CancellationToken ct = default);

        Task<List<PaymentRefund>> GetByPaymentAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<decimal> GetReservedAmountAsync(
            int paymentId,
            CancellationToken ct = default);

        Task AddAsync(
            PaymentRefund refund,
            CancellationToken ct = default);

        Task SaveAsync(CancellationToken ct = default);
    }
}