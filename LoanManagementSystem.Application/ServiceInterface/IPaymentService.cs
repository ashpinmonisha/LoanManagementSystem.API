using LoanManagementSystem.Application.DTOs.Payment;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface IPaymentService
    {
        // 4.1 Record Payment
        Task<PaymentResponseDto> RecordPaymentAsync(
            RecordPaymentDto dto);

        // 4.2 Payment History
        Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(
            int loanId);

        Task<List<PaymentHistoryDto>> GetCustomerPaymentHistoryAsync(
            int loanId,
            int customerId);
    }
}