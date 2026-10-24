using LoanManagementSystem.Application.DTOs.Refund;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Services
{
    public class PaymentRefundService : IPaymentRefundService
    {
        private readonly IPaymentRefundRepository _repository;
        private readonly IRefundGatewayClient _gateway;

        public PaymentRefundService(
            IPaymentRefundRepository repository,
            IRefundGatewayClient gateway)
        {
            _repository = repository;
            _gateway = gateway;
        }

        public async Task<RefundResponseDto> RequestAsync(
            CreateRefundDto dto,
            CancellationToken ct = default)
        {
            var payment = await _repository.GetPaymentAsync(
                dto.PaymentId, ct);

            if (payment == null ||
                payment.PaymentStatus != "Completed")
            {
                throw new InvalidOperationException(
                    "Only verified completed payments can be refunded.");
            }

            if (dto.Amount <= 0)
                throw new ArgumentException("Refund amount must be positive.");

            var reserved = await _repository.GetReservedAmountAsync(
                dto.PaymentId, ct);

            if (dto.Amount > payment.Amount - reserved)
            {
                throw new InvalidOperationException(
                    "Refund exceeds the remaining refundable amount.");
            }

            var refund = new PaymentRefund
            {
                PaymentId = payment.PaymentId,
                CustomerId = payment.CustomerId,
                LoanId = payment.LoanId,
                Amount = dto.Amount,
                Reason = dto.Reason,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            };

            await _repository.AddAsync(refund, ct);

            return Map(refund);
        }

        public async Task<RefundResponseDto> ApproveAsync(
            int refundId,
            string employeeId,
            CancellationToken ct = default)
        {
            var refund = await GetRequiredAsync(refundId, ct);

            if (refund.Status != "Pending")
                throw new InvalidOperationException(
                    "Only pending refunds can be approved.");

            refund.Status = "Approved";
            refund.ApprovedBy = employeeId;
            refund.ApprovedAt = DateTime.UtcNow;

            await _repository.SaveAsync(ct);

            return Map(refund);
        }

        public async Task<RefundResponseDto> RejectAsync(
            int refundId,
            string employeeId,
            string reason,
            CancellationToken ct = default)
        {
            var refund = await GetRequiredAsync(refundId, ct);

            if (refund.Status != "Pending")
                throw new InvalidOperationException(
                    "Only pending refunds can be rejected.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException(
                    "Rejection reason is required.");

            refund.Status = "Rejected";
            refund.RejectedBy = employeeId;
            refund.RejectionReason = reason;

            await _repository.SaveAsync(ct);

            return Map(refund);
        }

        public async Task<RefundResponseDto> ProcessAsync(
            int refundId,
            CancellationToken ct = default)
        {
            var refund = await GetRequiredAsync(refundId, ct);

            if (refund.Status == "Completed" ||
                refund.Status == "Processing")
            {
                return Map(refund);
            }

            if (refund.Status != "Approved")
                throw new InvalidOperationException(
                    "Refund must be approved before processing.");

            var payment = await _repository.GetPaymentAsync(
                refund.PaymentId, ct);

            if (payment == null ||
                payment.PaymentStatus != "Completed")
            {
                throw new InvalidOperationException(
                    "Completed payment not found.");
            }

            // In your Payment entity, PaymentReference must contain
            // the real Razorpay payment ID (pay_...).
            // Otherwise replace this with the correct verified
            // Razorpay payment ID lookup from your payment order table.
            var gatewayPaymentId = payment.PaymentReference;

            if (string.IsNullOrWhiteSpace(gatewayPaymentId) ||
                !gatewayPaymentId.StartsWith("pay_"))
            {
                throw new InvalidOperationException(
                    "Verified Razorpay payment ID is missing.");
            }

            refund.RazorpayPaymentId = gatewayPaymentId;
            refund.Status = "Processing";
            refund.ProcessedAt = DateTime.UtcNow;

            // Persist Processing before calling the external gateway.
            await _repository.SaveAsync(ct);

            try
            {
                var result = await _gateway.CreateRefundAsync(
                    gatewayPaymentId,
                    refund.Amount,
                    $"REF-{refund.RefundId}",
                    ct);

                if (result.Amount != refund.Amount)
                {
                    throw new InvalidOperationException(
                        "Gateway refund amount does not match the request.");
                }

                refund.RazorpayRefundId = result.RefundId;

                ApplyGatewayStatus(refund, result.Status);

                await _repository.SaveAsync(ct);
            }
            catch (Exception)
            {
                // Do not automatically retry the POST.
                // The gateway may have accepted the refund even if
                // the response was lost.
                refund.FailureReason =
                    "Gateway outcome uncertain. Reconcile before retrying.";

                await _repository.SaveAsync(CancellationToken.None);

                throw;
            }

            return Map(refund);
        }

        public async Task<RefundResponseDto> RefreshStatusAsync(
            int refundId,
            CancellationToken ct = default)
        {
            var refund = await GetRequiredAsync(refundId, ct);

            if (string.IsNullOrWhiteSpace(refund.RazorpayRefundId) ||
                string.IsNullOrWhiteSpace(refund.RazorpayPaymentId))
            {
                return Map(refund);
            }

            var result = await _gateway.GetRefundAsync(
                refund.RazorpayPaymentId,
                refund.RazorpayRefundId,
                ct);

            if (result.Amount != refund.Amount)
                throw new InvalidOperationException(
                    "Gateway refund amount mismatch.");

            ApplyGatewayStatus(refund, result.Status);

            await _repository.SaveAsync(ct);

            return Map(refund);
        }

        public async Task<List<RefundResponseDto>> GetHistoryAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            var refunds = await _repository.GetByPaymentAsync(
                paymentId, ct);

            return refunds.Select(Map).ToList();
        }

        private async Task<PaymentRefund> GetRequiredAsync(
            int refundId,
            CancellationToken ct)
        {
            return await _repository.GetByIdAsync(refundId, ct)
                ?? throw new KeyNotFoundException(
                    "Refund request not found.");
        }

        private static void ApplyGatewayStatus(
            PaymentRefund refund,
            string gatewayStatus)
        {
            switch (gatewayStatus.ToLowerInvariant())
            {
                case "processed":
                    refund.Status = "Completed";
                    refund.CompletedAt ??= DateTime.UtcNow;
                    refund.FailureReason = null;
                    break;

                case "failed":
                    refund.Status = "Failed";
                    refund.FailureReason =
                        "Gateway reported refund failure.";
                    break;

                default:
                    refund.Status = "Processing";
                    break;
            }
        }

        private static RefundResponseDto Map(PaymentRefund r)
        {
            return new RefundResponseDto
            {
                RefundId = r.RefundId,
                PaymentId = r.PaymentId,
                CustomerId = r.CustomerId,
                LoanId = r.LoanId,
                Amount = r.Amount,
                Reason = r.Reason,
                Status = r.Status,
                RazorpayRefundId = r.RazorpayRefundId,
                FailureReason = r.FailureReason,
                RequestedAt = r.RequestedAt,
                CompletedAt = r.CompletedAt
            };
        }
    }
}