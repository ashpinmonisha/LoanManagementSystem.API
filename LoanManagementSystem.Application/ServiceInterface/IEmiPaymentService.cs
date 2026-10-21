using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Interfaces
{
    public interface IEmiPaymentService
    {
        Task<EmiPaymentResultDto> AllocatePaymentAsync(
            EmiPaymentDto dto,
            CancellationToken cancellationToken = default);

        Task<EmiOutstandingDto> GetOutstandingAsync(
            int loanId,
            CancellationToken cancellationToken = default);
    }

    public interface IEmiPaymentRepository
    {
        Task<PaymentAllocation?> GetAllocationByPaymentIdAsync(
            int paymentId,
            CancellationToken cancellationToken = default);

        Task<Payment?> GetCompletedPaymentAsync(
            int paymentId,
            CancellationToken cancellationToken = default);

        Task AddAllocationAsync(
            PaymentAllocation allocation,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}    
