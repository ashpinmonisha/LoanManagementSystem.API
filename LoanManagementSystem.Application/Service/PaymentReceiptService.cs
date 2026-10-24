using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Application.Services
{
    public class PaymentReceiptService : IPaymentReceiptService
    {
        private readonly IPaymentReceiptRepository _repository;

        public PaymentReceiptService(
            IPaymentReceiptRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaymentReceiptDto> GenerateAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            if (paymentId <= 0)
                throw new ArgumentException("Invalid Payment ID.");

            var existing = await _repository
                .GetByPaymentIdAsync(paymentId, ct);

            if (existing != null)
                return Map(existing);

            var payment = await _repository
                .GetCompletedPaymentAsync(paymentId, ct);

            if (payment == null)
                throw new InvalidOperationException(
                    "Verified completed payment not found.");

            var receipt = new PaymentReceipt
            {
                ReceiptNumber =
                    $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}",

                PaymentId = payment.PaymentId,
                CustomerId = payment.CustomerId,
                LoanId = payment.LoanId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod ?? "",
                PaymentReference = payment.PaymentReference ?? "",
                GeneratedAt = DateTime.UtcNow
            };

            try
            {
                await _repository.AddAsync(receipt, ct);
                return Map(receipt);
            }
            catch (DbUpdateException)
            {
                // A concurrent request may have created the same receipt.
                // Check the database before returning an error.
                var saved = await _repository
                    .GetByPaymentIdAsync(paymentId, ct);

                if (saved != null)
                    return Map(saved);

                throw;
            }
        }

        public async Task<PaymentReceiptDto?> GetAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            var receipt = await _repository
                .GetByPaymentIdAsync(paymentId, ct);

            return receipt == null ? null : Map(receipt);
        }

        public async Task<List<PaymentReceiptDto>> GetByLoanAsync(
            int loanId,
            CancellationToken ct = default)
        {
            if (loanId <= 0)
                throw new ArgumentException("Invalid Loan ID.");

            var receipts = await _repository
                .GetByLoanIdAsync(loanId, ct);

            return receipts.Select(Map).ToList();
        }

        private static PaymentReceiptDto Map(PaymentReceipt receipt)
        {
            return new PaymentReceiptDto
            {
                Id = receipt.Id,
                ReceiptNumber = receipt.ReceiptNumber,
                PaymentId = receipt.PaymentId,
                CustomerId = receipt.CustomerId,
                LoanId = receipt.LoanId,
                Amount = receipt.Amount,
                PaymentDate = receipt.PaymentDate,
                PaymentMethod = receipt.PaymentMethod,
                PaymentReference = receipt.PaymentReference,
                GeneratedAt = receipt.GeneratedAt
            };
        }
    }
}