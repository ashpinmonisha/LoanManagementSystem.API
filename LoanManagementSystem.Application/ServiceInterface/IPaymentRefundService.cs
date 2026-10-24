using LoanManagementSystem.Application.DTOs.Refund;

namespace LoanManagementSystem.Application.Interfaces
{
    public interface IPaymentRefundService
    {
        Task<RefundResponseDto> RequestAsync(
            CreateRefundDto dto,
            CancellationToken ct = default);

        Task<RefundResponseDto> ApproveAsync(
            int refundId,
            string employeeId,
            CancellationToken ct = default);

        Task<RefundResponseDto> RejectAsync(
            int refundId,
            string employeeId,
            string reason,
            CancellationToken ct = default);

        Task<RefundResponseDto> ProcessAsync(
            int refundId,
            CancellationToken ct = default);

        Task<RefundResponseDto> RefreshStatusAsync(
            int refundId,
            CancellationToken ct = default);

        Task<List<RefundResponseDto>> GetHistoryAsync(
            int paymentId,
            CancellationToken ct = default);
    }
}