using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;


namespace LoanManagementSystem.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentNotificationQueue _notificationQueue;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IPaymentNotificationQueue notificationQueue)
        {
            _paymentRepository = paymentRepository;
            _notificationQueue = notificationQueue;
        }

        // 4.1 Record Payment
        public async Task<PaymentResponseDto> RecordPaymentAsync(
            RecordPaymentDto dto)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException(
                    "Payment amount must be greater than zero");

            if (string.IsNullOrWhiteSpace(dto.PaymentReference))
                throw new ArgumentException(
                    "Payment reference is required");

            var existingPayment =
                await _paymentRepository.GetByReferenceAsync(
                    dto.PaymentReference);

            if (existingPayment != null)
                throw new InvalidOperationException(
                    "Payment reference already exists");

            var payment = new Payment
            {
                CustomerId = dto.CustomerId,
                LoanId = dto.LoanId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                PaymentMethod = dto.PaymentMethod,
                PaymentReference = dto.PaymentReference,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            // Only verified successful payments should
            // trigger success notifications.
            if (payment.PaymentStatus == "Completed")
            {
                await _notificationQueue.QueueAsync(
                    payment.PaymentId,
                    payment.CustomerId,
                    payment.LoanId,
                    payment.Amount,
                    payment.PaymentReference);
            }

            return new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                CustomerId = payment.CustomerId,
                LoanId = payment.LoanId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                PaymentReference = payment.PaymentReference,
                PaymentStatus = payment.PaymentStatus
            };
        }

        // 4.2 Get Payment History
        public async Task<List<PaymentHistoryDto>>
            GetPaymentHistoryAsync(int loanId)
        {
            if (loanId <= 0)
                throw new ArgumentException("Invalid Loan ID");

            var payments = await _paymentRepository
                .GetPaymentHistoryAsync(loanId);

            return payments.Select(p => new PaymentHistoryDto
            {
                PaymentId = p.PaymentId,
                CustomerId = p.CustomerId,
                LoanId = p.LoanId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                PaymentReference = p.PaymentReference,
                PaymentStatus = p.PaymentStatus
            }).ToList();
        }

        // 4.2 Get Customer Payment History
        public async Task<List<PaymentHistoryDto>>
            GetCustomerPaymentHistoryAsync(
                int loanId,
                int customerId)
        {
            if (loanId <= 0 || customerId <= 0)
                throw new ArgumentException(
                    "Invalid Loan ID or Customer ID");

            var payments = await _paymentRepository
                .GetCustomerPaymentHistoryAsync(
                    loanId, customerId);

            return payments.Select(p => new PaymentHistoryDto
            {
                PaymentId = p.PaymentId,
                CustomerId = p.CustomerId,
                LoanId = p.LoanId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                PaymentReference = p.PaymentReference,
                PaymentStatus = p.PaymentStatus
            }).ToList();
        }
    }
}