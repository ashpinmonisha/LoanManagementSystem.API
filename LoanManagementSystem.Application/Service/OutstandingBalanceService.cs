
using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;

namespace LoanManagementSystem.Application.Services
{
    public class OutstandingBalanceService
        : IOutstandingBalanceService
    {
        private readonly ILoanOutstandingDataReader _reader;
        private readonly IPaymentRepository _paymentRepository;

        public OutstandingBalanceService(
            ILoanOutstandingDataReader reader,
            IPaymentRepository paymentRepository)
        {
            _reader = reader
                ?? throw new ArgumentNullException(nameof(reader));

            _paymentRepository = paymentRepository
                ?? throw new ArgumentNullException(nameof(paymentRepository));
        }

        // ==========================================
        // MODULE 4.3 - OUTSTANDING BALANCE
        // ==========================================
        public async Task<OutstandingBalanceDto>
            GetOutstandingBalanceAsync(int loanId)
        {
            // STEP 1 - Validate Loan ID
            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid Loan ID.",
                    nameof(loanId));
            }

            // STEP 2 - Get loan balance
            var loan = await _reader.GetLoanBalanceAsync(loanId);

            if (loan == null)
            {
                throw new KeyNotFoundException(
                    $"Loan balance not found for Loan ID {loanId}.");
            }

            // STEP 3 - Validate stored balances
            if (loan.RemainingPrincipal < 0 ||
                loan.RemainingInterest < 0)
            {
                throw new InvalidOperationException(
                    $"Invalid negative loan balance for Loan ID {loanId}.");
            }

            // STEP 4 - Read EMI schedule
            var emis = await _reader.GetEmisAsync(loanId);

            decimal pendingEmiAmount = 0m;
            decimal overdueAmount = 0m;

            // An active loan must have an initialized schedule.
            if (emis == null || emis.Count == 0)
            {
                if (loan.RemainingPrincipal > 0 ||
                    loan.RemainingInterest > 0)
                {
                    throw new InvalidOperationException(
                        $"EMI schedule is missing for Loan ID {loanId}. " +
                        "Initialize the EMI installments before calculating " +
                        "the complete outstanding balance.");
                }
            }
            else
            {
                DateTime today = DateTime.UtcNow.Date;

                // STEP 5 - Pending EMI
                pendingEmiAmount = emis.Sum(e =>
                    Math.Max(0m, e.RemainingAmount));

                // STEP 6 - Overdue EMI
                overdueAmount = emis
                    .Where(e =>
                        e.DueDate.Date < today &&
                        e.RemainingAmount > 0m)
                    .Sum(e => e.RemainingAmount);
            }

            // STEP 7 - Get unpaid penalties
            decimal penaltyAmount =
                await _paymentRepository.GetPenaltyTotalAsync(loanId);

            if (penaltyAmount < 0)
            {
                throw new InvalidOperationException(
                    $"Invalid penalty balance for Loan ID {loanId}.");
            }

            // STEP 8 - Loan balance
            decimal remainingPrincipal =
                loan.RemainingPrincipal;

            decimal remainingInterest =
                loan.RemainingInterest;

            // STEP 9 - Avoid double-counting EMI
            decimal totalOutstanding =
                remainingPrincipal +
                remainingInterest +
                penaltyAmount;

            // STEP 10 - Return result
            return new OutstandingBalanceDto
            {
                LoanId = loanId,
                RemainingPrincipal = remainingPrincipal,
                RemainingInterest = remainingInterest,
                PendingEmiAmount = pendingEmiAmount,
                OverdueAmount = overdueAmount,
                PenaltyAmount = penaltyAmount,
                TotalOutstanding = totalOutstanding
            };
        }
    }
}
