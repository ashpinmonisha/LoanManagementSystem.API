using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;

namespace LoanManagementSystem.Application.Services
{
    public class OutstandingBalanceService : IOutstandingBalanceService
    {
        private readonly ILoanModuleClient _loanModuleClient;
        private readonly IPaymentRepository _paymentRepository;

        public OutstandingBalanceService(
            ILoanModuleClient loanModuleClient,
            IPaymentRepository paymentRepository)
        {
            _loanModuleClient = loanModuleClient;
            _paymentRepository = paymentRepository;
        }

        public async Task<OutstandingBalanceDto> GetOutstandingBalanceAsync(
            int loanId)
        {
            if (loanId <= 0)
                throw new ArgumentException("Invalid Loan ID");

            // Get loan details from Moni's module
            var loan = await _loanModuleClient
                .GetLoanBalanceAsync(loanId);

            if (loan == null)
                throw new KeyNotFoundException("Loan not found");

            // Get EMI schedule from Jousha's module
            var emis = await _loanModuleClient
                .GetEmisAsync(loanId);

            // Calculate pending EMI
            decimal pendingEmi = emis
                .Sum(e => e.RemainingAmount);

            // Calculate overdue EMI
            var today = DateTime.UtcNow.Date;

            decimal overdueAmount = emis
                .Where(e =>
                    e.DueDate.Date < today &&
                    e.RemainingAmount > 0)
                .Sum(e => e.RemainingAmount);

            // Get unpaid penalty amount
            decimal penaltyAmount = await _paymentRepository
                .GetPenaltyTotalAsync(loanId);

            // Remaining principal and interest
            decimal remainingPrincipal = loan.RemainingPrincipal;
            decimal remainingInterest = loan.RemainingInterest;

            // Avoid counting pending EMI twice
            decimal totalOutstanding =
                remainingPrincipal +
                remainingInterest +
                penaltyAmount;

            return new OutstandingBalanceDto
            {
                LoanId = loanId,
                RemainingPrincipal = remainingPrincipal,
                RemainingInterest = remainingInterest,
                PendingEmiAmount = pendingEmi,
                OverdueAmount = overdueAmount,
                PenaltyAmount = penaltyAmount,
                TotalOutstanding = totalOutstanding
            };
        }
    }
}