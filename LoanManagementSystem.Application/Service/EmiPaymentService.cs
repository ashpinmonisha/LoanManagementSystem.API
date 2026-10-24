using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Services
{
    public class EmiPaymentService : IEmiPaymentService
    {
        private readonly IEmiPaymentRepository _repository;
        private readonly IEmiModuleClient _emiClient;

        public EmiPaymentService(
            IEmiPaymentRepository repository,
            IEmiModuleClient emiClient)
        {
            _repository = repository;
            _emiClient = emiClient;
        }

        public async Task<EmiPaymentResultDto> AllocatePaymentAsync(
            EmiPaymentDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.PaymentId <= 0 || dto.EmiId <= 0)
                throw new ArgumentException(
                    "PaymentId and EmiId must be greater than zero.");

            var allocation =
                await _repository.GetAllocationByPaymentIdAsync(
                    dto.PaymentId, cancellationToken);

            if (allocation != null)
            {
                if (allocation.EmiId != dto.EmiId)
                    throw new InvalidOperationException(
                        "Payment already assigned to another EMI.");

                var previous =
                    await _emiClient.GetAllocationAsync(
                        allocation.IdempotencyKey,
                        cancellationToken);

                if (previous == null)
                    throw new InvalidOperationException(
                        "Allocation is pending reconciliation.");

                ValidateRemote(previous, allocation);

                if (allocation.Status != "Applied")
                {
                    allocation.Status = "Applied";
                    allocation.AppliedAt = DateTime.UtcNow;
                    allocation.LastError = null;

                    await _repository.SaveChangesAsync(
                        cancellationToken);
                }

                return ToResult(allocation, previous);
            }

            // Important:
            // Add GetCompletedPaymentAsync to the repository.
            // This must read the existing Payments table.
            var payment = await _repository.GetCompletedPaymentAsync(
                dto.PaymentId, cancellationToken);

            if (payment == null)
                throw new InvalidOperationException(
                    "Completed payment not found.");

            var emi = await _emiClient.GetEmiAsync(
                dto.EmiId, cancellationToken);

            if (emi == null)
                throw new InvalidOperationException("EMI not found.");

            if (payment.LoanId != emi.LoanId)
                throw new InvalidOperationException(
                    "Payment loan does not match EMI loan.");

            if (payment.EmiId != dto.EmiId)
                throw new InvalidOperationException(
                    "Payment belongs to another EMI.");

            if (payment.Amount <= 0 ||
                payment.Amount > emi.RemainingAmount)
                throw new InvalidOperationException(
                    "Payment amount exceeds the remaining EMI.");

            allocation = new PaymentAllocation
            {
                PaymentId = payment.PaymentId,
                CustomerId = payment.CustomerId,
                LoanId = payment.LoanId,
                EmiId = dto.EmiId,
                Amount = payment.Amount,
                IdempotencyKey =
                    $"loan-payment-{payment.PaymentId}",
                Status = "Pending"
            };

            await _repository.AddAllocationAsync(
                allocation, cancellationToken);

            await _repository.SaveChangesAsync(
                cancellationToken);

            // Jousha's API must atomically deduplicate this key.
            var result = await _emiClient.ApplyPaymentAsync(
                new ApplyEmiPaymentRequestDto
                {
                    PaymentId = payment.PaymentId,
                    EmiId = dto.EmiId,
                    Amount = payment.Amount,
                    IdempotencyKey = allocation.IdempotencyKey
                },
                cancellationToken);

            if (result == null)
                throw new InvalidOperationException(
                    "Allocation pending or rejected. Check Jousha's API.");

            ValidateRemote(result, allocation);

            allocation.Status = "Applied";
            allocation.AppliedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync(
                cancellationToken);

            return ToResult(allocation, result);
        }

        public Task<EmiOutstandingDto> GetOutstandingAsync(
            int loanId,
            CancellationToken cancellationToken = default)
        {
            if (loanId <= 0)
                throw new ArgumentException("Invalid LoanId.");

            return _emiClient.GetOutstandingAsync(
                loanId, cancellationToken);
        }

        private static void ValidateRemote(
            ApplyEmiPaymentResponseDto result,
            PaymentAllocation allocation)
        {
            if (result.EmiId != allocation.EmiId ||
                result.AppliedAmount != allocation.Amount)
            {
                throw new InvalidOperationException(
                    "EMI API allocation response mismatch.");
            }
        }

        private static EmiPaymentResultDto ToResult(
            PaymentAllocation allocation,
            ApplyEmiPaymentResponseDto result)
        {
            return new EmiPaymentResultDto
            {
                AllocationId = allocation.Id,
                PaymentId = allocation.PaymentId,
                EmiId = allocation.EmiId,
                AllocatedAmount = allocation.Amount,
                RemainingAmount = result.RemainingAmount,
                AllocationStatus = allocation.Status,
                EmiStatus = result.Status
            };
        }
    }
}