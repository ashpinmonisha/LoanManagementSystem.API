using LoanManagementSystem.Application.DTOs.Reconciliation;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Services
{
    public class PaymentReconciliationService
        : IPaymentReconciliationService
    {
        private readonly IPaymentReconciliationRepository _repository;
        private readonly IPaymentGatewayVerificationClient _gateway;

        public PaymentReconciliationService(
            IPaymentReconciliationRepository repository,
            IPaymentGatewayVerificationClient gateway)
        {
            _repository = repository;
            _gateway = gateway;
        }

        public async Task<ReconciliationResponseDto> VerifyAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            var payment = await _repository.GetPaymentAsync(
                paymentId, ct);

            if (payment == null)
                throw new KeyNotFoundException(
                    $"Payment {paymentId} was not found.");

            var reference = payment.PaymentReference?.Trim() ?? "";

            var reconciliation = new PaymentReconciliation
            {
                PaymentId = payment.PaymentId,
                LoanId = payment.LoanId,
                CustomerId = payment.CustomerId,
                PaymentReference = reference,
                DatabaseAmount = payment.Amount,
                DatabaseStatus = payment.PaymentStatus ?? "",
                ReconciledAt = DateTime.UtcNow
            };

            if (!reference.StartsWith(
                "pay_", StringComparison.Ordinal))
            {
                reconciliation.Result = "InvalidReference";
                reconciliation.Remarks =
                    "No verified Razorpay payment ID is stored.";

                await _repository.AddAsync(reconciliation, ct);
                return Map(reconciliation);
            }

            var duplicateCount =
                await _repository.CountReferenceAsync(reference, ct);

            // Always retrieve the gateway record before deciding
            // whether an otherwise valid payment matches.
            var gatewayPayment =
                await _gateway.GetPaymentAsync(reference, ct);

            if (gatewayPayment == null)
            {
                reconciliation.Result = "GatewayNotFound";
                reconciliation.Remarks =
                    "Razorpay payment ID was not found.";
            }
            else
            {
                reconciliation.GatewayAmount = gatewayPayment.Amount;
                reconciliation.GatewayStatus = gatewayPayment.Status;

                if (!gatewayPayment.Currency.Equals(
                    "INR", StringComparison.OrdinalIgnoreCase))
                {
                    reconciliation.Result = "StatusMismatch";
                    reconciliation.Remarks =
                        "Unexpected gateway currency: " +
                        gatewayPayment.Currency;
                }
                else if (duplicateCount > 1)
                {
                    reconciliation.Result = "DuplicateReference";
                    reconciliation.Remarks =
                        $"{duplicateCount} local payments share this reference.";
                }
                else if (payment.Amount != gatewayPayment.Amount)
                {
                    reconciliation.Result = "AmountMismatch";
                    reconciliation.Remarks =
                        "Database amount differs from Razorpay amount.";
                }
                else if (gatewayPayment.Status.Equals(
                    "captured", StringComparison.OrdinalIgnoreCase) &&
                    payment.PaymentStatus.Equals(
                        "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    reconciliation.Result = "Matched";
                    reconciliation.Remarks =
                        "Payment amount and completed status verified.";
                }
                else if (gatewayPayment.Status.Equals(
                    "created", StringComparison.OrdinalIgnoreCase) ||
                    gatewayPayment.Status.Equals(
                        "authorized", StringComparison.OrdinalIgnoreCase))
                {
                    reconciliation.Result = "Pending";
                    reconciliation.Remarks =
                        "Gateway payment is not yet captured.";
                }
                else
                {
                    reconciliation.Result = "StatusMismatch";
                    reconciliation.Remarks =
                        $"Database: {payment.PaymentStatus}; " +
                        $"Razorpay: {gatewayPayment.Status}.";
                }
            }

            await _repository.AddAsync(reconciliation, ct);

            return Map(reconciliation);
        }

        public async Task<List<ReconciliationResponseDto>> BulkAsync(
            int limit,
            CancellationToken ct = default)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentException(
                    "Limit must be between 1 and 100.");

            var payments = await _repository.GetPaymentsAsync(
                limit, ct);

            var results = new List<ReconciliationResponseDto>();

            foreach (var payment in payments)
            {
                ct.ThrowIfCancellationRequested();

                // Gateway errors stop the batch rather than
                // incorrectly marking payments as mismatched.
                var result = await VerifyAsync(
                    payment.PaymentId, ct);

                results.Add(result);
            }

            return results;
        }

        public async Task<List<ReconciliationResponseDto>> HistoryAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            var records = await _repository.GetHistoryAsync(
                paymentId, ct);

            return records.Select(Map).ToList();
        }

        public async Task<List<ReconciliationResponseDto>>
            MismatchesAsync(CancellationToken ct = default)
        {
            var records =
                await _repository.GetLatestMismatchesAsync(ct);

            return records.Select(Map).ToList();
        }

        public async Task<ReconciliationReportDto> ReportAsync(
            CancellationToken ct = default)
        {
            var records =
                await _repository.GetLatestResultsAsync(ct);

            return new ReconciliationReportDto
            {
                TotalPaymentsChecked = records.Count,
                Matched = records.Count(r => r.Result == "Matched"),
                AmountMismatches = records.Count(
                    r => r.Result == "AmountMismatch"),
                StatusMismatches = records.Count(
                    r => r.Result == "StatusMismatch"),
                DuplicateReferences = records.Count(
                    r => r.Result == "DuplicateReference"),
                Pending = records.Count(r => r.Result == "Pending"),
                GatewayNotFound = records.Count(
                    r => r.Result == "GatewayNotFound"),
                InvalidReferences = records.Count(
                    r => r.Result == "InvalidReference")
            };
        }

        private static ReconciliationResponseDto Map(
            PaymentReconciliation r)
        {
            return new ReconciliationResponseDto
            {
                ReconciliationId = r.ReconciliationId,
                PaymentId = r.PaymentId,
                LoanId = r.LoanId,
                PaymentReference = r.PaymentReference,
                DatabaseAmount = r.DatabaseAmount,
                GatewayAmount = r.GatewayAmount,
                DatabaseStatus = r.DatabaseStatus,
                GatewayStatus = r.GatewayStatus,
                Result = r.Result,
                Remarks = r.Remarks,
                ReconciledAt = r.ReconciledAt
            };
        }
    }
}