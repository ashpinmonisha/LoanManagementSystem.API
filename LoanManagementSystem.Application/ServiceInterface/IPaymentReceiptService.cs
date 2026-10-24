using LoanManagementSystem.Application.DTOs;

namespace LoanManagementSystem.Application.Interfaces
{
    public interface IPaymentReceiptService
    {
        Task<PaymentReceiptDto> GenerateAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<PaymentReceiptDto?> GetAsync(
            int paymentId,
            CancellationToken ct = default);

        Task<List<PaymentReceiptDto>> GetByLoanAsync(
            int loanId,
            CancellationToken ct = default);
    }
}