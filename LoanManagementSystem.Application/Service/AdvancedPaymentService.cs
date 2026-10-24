using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Services
{
    public class AdvancedPaymentService : IAdvancedPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILoanModuleClient _loanClient;
        private readonly IEmiAllocationService _emiAllocationService;

        public AdvancedPaymentService(
            IPaymentRepository paymentRepository,
            ILoanModuleClient loanClient,
            IEmiAllocationService emiAllocationService)
        {
            _paymentRepository = paymentRepository;
            _loanClient = loanClient;
            _emiAllocationService = emiAllocationService;
        }

        // 4.4.1 Overdue EMI Identification
        public async Task<List<OverdueEmiDto>> GetOverdueEmisAsync(
            int loanId)
        {
            if (loanId <= 0)
                throw new ArgumentException("Invalid Loan ID");

            var loan = await _loanClient.GetLoanBalanceAsync(loanId);

            if (loan == null)
                throw new KeyNotFoundException("Loan not found");

            var emis = await _loanClient.GetEmisAsync(loanId);

            var today = DateTime.UtcNow.Date;

            return emis
                .Where(e =>
                    e.DueDate.Date < today &&
                    e.RemainingAmount > 0)
                .Select(e => new OverdueEmiDto
                {
                    EmiId = e.EmiId,
                    LoanId = e.LoanId,
                    DueDate = e.DueDate,
                    DaysOverdue =
                        (today - e.DueDate.Date).Days,
                    OverdueAmount = e.RemainingAmount
                })
                .ToList();
        }

        // 4.4.2 Create Penalty
        public async Task<int> CreatePenaltyAsync(
            CreatePenaltyDto dto)
        {
            var emis = await _loanClient.GetEmisAsync(dto.LoanId);

            var emi = emis.FirstOrDefault(e => e.EmiId == dto.EmiId);

            if (emi == null || emi.LoanId != dto.LoanId)
                throw new ArgumentException(
                    "EMI does not belong to this loan");

            int daysOverdue =
                (DateTime.UtcNow.Date - emi.DueDate.Date).Days;

            if (daysOverdue <= 0 || emi.RemainingAmount <= 0)
                throw new InvalidOperationException(
                    "This EMI is not overdue");

            var existing = await _paymentRepository
                .GetPenaltyByEmiAsync(dto.EmiId);

            if (existing != null)
                throw new InvalidOperationException(
                    "Penalty already exists for this EMI");

            var penalty = new Penalty
            {
                LoanId = dto.LoanId,
                EmiId = dto.EmiId,
                Amount = dto.Amount,
                PaidAmount = 0,
                DaysOverdue = daysOverdue,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddPenaltyAsync(penalty);
            await _paymentRepository.SaveChangesAsync();

            return penalty.PenaltyId;
        }

        // Requires verified payment + allocation workflow
        public async Task<EmiAllocationResult> AllocatePartialPaymentAsync(
            PartialPaymentDto dto)
        {
            if (dto.LoanId <= 0 || dto.EmiId <= 0 || dto.CustomerId <= 0 ||
                dto.Amount <= 0 || string.IsNullOrWhiteSpace(dto.PaymentReference))
            {
                throw new ArgumentException("Valid payment, loan, EMI, customer, and amount are required.");
            }

            var payment = await _paymentRepository
                .GetByReferenceAsync(dto.PaymentReference.Trim());

            if (payment == null)
                throw new KeyNotFoundException("Payment not found.");

            if (payment.CustomerId != dto.CustomerId || payment.LoanId != dto.LoanId)
                throw new InvalidOperationException("Payment does not belong to the supplied customer and loan.");

            if (payment.Amount != dto.Amount)
                throw new InvalidOperationException("Payment amount does not match the recorded payment.");

            var allocation = await _emiAllocationService.AllocateAsync(
                new AllocateEmiPaymentDto
                {
                    PaymentId = payment.PaymentId,
                    EmiId = dto.EmiId
                });

            payment.EmiId = dto.EmiId;
            await _paymentRepository.SaveChangesAsync();

            return new EmiAllocationResult
            {
                EmiId = allocation.EmiId,
                AllocatedAmount = allocation.AllocatedAmount,
                RemainingAmount = allocation.RemainingEmiAmount,
                Success = allocation.AllocationStatus == "Applied"
            };
        }

        // Requires trusted payment-provider verification
        public async Task<bool> ReconcilePaymentAsync(
            ReconcilePaymentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PaymentReference) ||
                string.IsNullOrWhiteSpace(dto.ProviderTransactionId))
                throw new ArgumentException("Payment reference and provider transaction ID are required.");

            var payment = await _paymentRepository
                .GetByReferenceAsync(dto.PaymentReference.Trim());

            if (payment == null)
                throw new KeyNotFoundException("Payment not found.");

            if (!string.Equals(payment.PaymentReference,
                    dto.ProviderTransactionId.Trim(),
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Provider transaction does not match the recorded payment reference.");

            return string.Equals(payment.PaymentStatus,
                "Completed", StringComparison.OrdinalIgnoreCase);
        }

        // Requires loan closure integration and concurrency protection
        public async Task<bool> CloseLoanAsync(int loanId)
        {
            if (loanId <= 0)
                throw new ArgumentException("Invalid Loan ID.");

            var balance = await _loanClient.GetLoanBalanceAsync(loanId);

            if (balance == null)
                throw new KeyNotFoundException("Loan not found.");

            if (balance.RemainingPrincipal > 0 || balance.RemainingInterest > 0)
                throw new InvalidOperationException(
                    "Loan cannot be closed while an outstanding balance remains.");

            if (string.Equals(balance.Status, "Closed", StringComparison.OrdinalIgnoreCase))
                return true;

            return await _loanClient.CloseLoanAsync(loanId);
        }
    }
}