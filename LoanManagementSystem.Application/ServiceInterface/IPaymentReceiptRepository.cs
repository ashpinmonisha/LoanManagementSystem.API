using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Interfaces
{
    public interface IPaymentReceiptRepository
    {
        Task<Payment?> GetCompletedPaymentAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<PaymentReceipt?> GetByPaymentIdAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<List<PaymentReceipt>> GetByLoanIdAsync(
            int loanId,
            CancellationToken ct = default);

        Task AddAsync(
            PaymentReceipt receipt,
            CancellationToken ct = default);
    }
}