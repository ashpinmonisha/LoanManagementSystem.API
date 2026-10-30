
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.DTOs.Payment;
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
        private readonly ILoanOutstandingDataReader _outstandingReader;

        // ONLY ONE CONSTRUCTOR
        public AdvancedPaymentService(
            IPaymentRepository paymentRepository,
            ILoanModuleClient loanClient,
            IEmiAllocationService emiAllocationService,
            ILoanOutstandingDataReader outstandingReader)
        {
            _paymentRepository = paymentRepository;
            _loanClient = loanClient;
            _emiAllocationService = emiAllocationService;
            _outstandingReader = outstandingReader;
        }

        // ============================================
        // 4.4.1 - OVERDUE EMI IDENTIFICATION
        // ============================================
        public async Task<List<OverdueEmiDto>> GetOverdueEmisAsync(
            int loanId)
        {
            if (loanId <= 0)
                throw new ArgumentException("Invalid Loan ID.");

            // Read loan balance from database
            var loan = await _outstandingReader
                .GetLoanBalanceAsync(loanId);

            if (loan == null)
                throw new KeyNotFoundException(
                    $"Loan ID {loanId} not found.");

            // Read EMI schedule directly from database
            // No HTTP request, so no internal JWT 401
            var emis = await _outstandingReader
                .GetEmisAsync(loanId);

            if (emis == null || emis.Count == 0)
            {
                throw new InvalidOperationException(
                    $"EMI schedule not initialized for Loan ID {loanId}.");
            }

            DateTime today = DateTime.UtcNow.Date;

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

        // ============================================
        // 4.4.2 - CREATE PENALTY
        // ============================================
        public async Task<int> CreatePenaltyAsync(
            CreatePenaltyDto dto)
        {
            if (dto == null)
                throw new ArgumentException(
                    "Penalty details are required.");

            if (dto.LoanId <= 0 || dto.EmiId <= 0)
            {
                throw new ArgumentException(
                    "Valid Loan ID and EMI ID are required.");
            }

            if (dto.Amount <= 0)
            {
                throw new ArgumentException(
                    "Penalty amount must be greater than zero.");
            }

            // STEP 1: Check loan exists
            var loan = await _outstandingReader
                .GetLoanBalanceAsync(dto.LoanId);

            if (loan == null)
            {
                throw new KeyNotFoundException(
                    $"Loan ID {dto.LoanId} not found.");
            }

            // STEP 2: Get EMI details directly from database
            var emis = await _outstandingReader
                .GetEmisAsync(dto.LoanId);

            if (emis == null || emis.Count == 0)
            {
                throw new InvalidOperationException(
                    "EMI schedule not initialized.");
            }

            // STEP 3: Find the requested EMI
            var emi = emis.FirstOrDefault(
                e => e.EmiId == dto.EmiId);

            if (emi == null || emi.LoanId != dto.LoanId)
            {
                throw new KeyNotFoundException(
                    "EMI not found for this loan.");
            }

            // STEP 4: Check overdue days
            DateTime today = DateTime.UtcNow.Date;

            int daysOverdue =
                (today - emi.DueDate.Date).Days;

            if (daysOverdue <= 0 ||
                emi.RemainingAmount <= 0)
            {
                throw new InvalidOperationException(
                    "This EMI is not overdue.");
            }

            // STEP 5: Prevent duplicate penalties
            var existing = await _paymentRepository
                .GetPenaltyByEmiAsync(dto.EmiId);

            if (existing != null)
            {
                throw new InvalidOperationException(
                    "Penalty already exists for this EMI.");
            }

            // STEP 6: Save penalty
            // Amount must follow your approved penalty policy
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

        // ============================================
        // 4.4.3 - PARTIAL PAYMENT ALLOCATION
        // ============================================
        public async Task<EmiAllocationResult>
            AllocatePartialPaymentAsync(PartialPaymentDto dto)
        {
            if (dto == null ||
                dto.LoanId <= 0 ||
                dto.EmiId <= 0 ||
                dto.CustomerId <= 0 ||
                dto.Amount <= 0 ||
                string.IsNullOrWhiteSpace(dto.PaymentReference))
            {
                throw new ArgumentException(
                    "Valid loan, customer, EMI, amount " +
                    "and payment reference are required.");
            }

            // STEP 1: Find recorded payment
            var payment = await _paymentRepository
                .GetByReferenceAsync(dto.PaymentReference.Trim());

            if (payment == null)
            {
                throw new KeyNotFoundException(
                    "Payment not found.");
            }

            // STEP 2: Verify payment ownership
            if (payment.CustomerId != dto.CustomerId ||
                payment.LoanId != dto.LoanId)
            {
                throw new InvalidOperationException(
                    "Payment does not belong to this customer and loan.");
            }

            // STEP 3: Verify payment amount
            if (payment.Amount != dto.Amount)
            {
                throw new InvalidOperationException(
                    "Payment amount does not match the recorded payment.");
            }

            // STEP 4: Require completed payment
            if (!string.Equals(
                payment.PaymentStatus,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only completed payments can be allocated.");
            }

            // STEP 5: Verify EMI belongs to this loan
            var emis = await _outstandingReader
                .GetEmisAsync(dto.LoanId);

            var emi = emis.FirstOrDefault(
                x => x.EmiId == dto.EmiId &&
                     x.LoanId == dto.LoanId);

            if (emi == null)
            {
                throw new KeyNotFoundException(
                    "EMI not found for this loan.");
            }

            if (emi.RemainingAmount <= 0)
            {
                throw new InvalidOperationException(
                    "This EMI has already been fully paid.");
            }

            if (dto.Amount > emi.RemainingAmount)
            {
                throw new InvalidOperationException(
                    "Payment exceeds the EMI remaining amount.");
            }

            // STEP 6: Allocate payment
            // Allocation service must enforce idempotency
            var allocation = await _emiAllocationService
                .AllocateAsync(new AllocateEmiPaymentDto
                {
                    PaymentId = payment.PaymentId,
                    EmiId = dto.EmiId
                });

            bool applied = string.Equals(
                allocation.AllocationStatus,
                "Applied",
                StringComparison.OrdinalIgnoreCase);

            if (!applied)
            {
                throw new InvalidOperationException(
                    "EMI allocation was not applied.");
            }

            // STEP 7: Update payment EMI reference
            payment.EmiId = dto.EmiId;

            await _paymentRepository.SaveChangesAsync();

            return new EmiAllocationResult
            {
                EmiId = allocation.EmiId,
                AllocatedAmount = allocation.AllocatedAmount,
                RemainingAmount = allocation.RemainingEmiAmount,
                Success = true
            };
        }

        // ============================================
        // 4.4.4 - PAYMENT RECONCILIATION
        // ============================================
        public async Task<bool> ReconcilePaymentAsync(
            ReconcilePaymentDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.PaymentReference) ||
                string.IsNullOrWhiteSpace(dto.ProviderTransactionId))
            {
                throw new ArgumentException(
                    "Payment reference and provider " +
                    "transaction ID are required.");
            }

            var payment = await _paymentRepository
                .GetByReferenceAsync(dto.PaymentReference.Trim());

            if (payment == null)
            {
                throw new KeyNotFoundException(
                    "Payment not found.");
            }

            // Local reference comparison only
            if (!string.Equals(
                payment.PaymentReference,
                dto.ProviderTransactionId.Trim(),
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Transaction ID does not match the recorded reference.");
            }

            return string.Equals(
                payment.PaymentStatus,
                "Completed",
                StringComparison.OrdinalIgnoreCase);
        }

        // ============================================
        // 4.4.5 - LOAN CLOSURE
        // ============================================
        public async Task<bool> CloseLoanAsync(int loanId)
        {
            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid Loan ID.");
            }

            // STEP 1: Read current balance
            var balance = await _outstandingReader
                .GetLoanBalanceAsync(loanId);

            if (balance == null)
            {
                throw new KeyNotFoundException(
                    $"Loan ID {loanId} not found.");
            }

            if (string.Equals(
                balance.Status,
                "Closed",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // STEP 2: Check remaining principal/interest
            if (balance.RemainingPrincipal != 0 ||
                balance.RemainingInterest != 0)
            {
                throw new InvalidOperationException(
                    "Loan cannot close while principal " +
                    "or interest remains outstanding.");
            }

            // STEP 3: Check unpaid EMIs
            var emis = await _outstandingReader
                .GetEmisAsync(loanId);

            if (emis.Count == 0)
            {
                throw new InvalidOperationException(
                    "EMI schedule is not initialized.");
            }

            if (emis.Any(e => e.RemainingAmount > 0))
            {
                throw new InvalidOperationException(
                    "Loan has unpaid EMI installments.");
            }

            // STEP 4: Check unpaid penalties
            decimal unpaidPenalty = await _paymentRepository
                .GetPenaltyTotalAsync(loanId);

            if (unpaidPenalty > 0)
            {
                throw new InvalidOperationException(
                    "Loan has unpaid penalties.");
            }

            // STEP 5: Close via Loan Module
            // This endpoint must exist and be authenticated.
            return await _loanClient.CloseLoanAsync(loanId);
        }
    }
}
