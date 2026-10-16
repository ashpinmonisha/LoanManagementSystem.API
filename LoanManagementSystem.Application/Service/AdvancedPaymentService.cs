using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Services
{
    public class AdvancedPaymentService : IAdvancedPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILoanModuleClient _loanClient;

        public AdvancedPaymentService(
            IPaymentRepository paymentRepository,
            ILoanModuleClient loanClient)
        {
            _paymentRepository = paymentRepository;
            _loanClient = loanClient;
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
        public Task<EmiAllocationResult> AllocatePartialPaymentAsync(
            PartialPaymentDto dto)
        {
            throw new NotImplementedException(
                "Partial payment requires verified payment " +
                "and idempotent EMI allocation integration.");
        }

        // Requires trusted payment-provider verification
        public Task<bool> ReconcilePaymentAsync(
            ReconcilePaymentDto dto)
        {
            throw new NotImplementedException(
                "Reconciliation requires provider verification " +
                "and an idempotent payment state transition.");
        }

        // Requires loan closure integration and concurrency protection
        public Task<bool> CloseLoanAsync(int loanId)
        {
            throw new NotImplementedException(
                "Loan closure requires verified balances " +
                "and an idempotent loan closure operation.");
        }
    }
}