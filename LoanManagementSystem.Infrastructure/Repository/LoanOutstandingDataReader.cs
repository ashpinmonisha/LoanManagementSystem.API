using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repositories
{
    public class LoanOutstandingDataReader
        : ILoanOutstandingDataReader
    {
        private readonly ApplicationDbContext _context;

        public LoanOutstandingDataReader(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoanBalanceInfo?> GetLoanBalanceAsync(
            int loanId)
        {
            var loan = await _context.LoanApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.LoanApplicationId == loanId);

            if (loan == null)
                return null;

            if (!string.Equals(
                loan.Status,
                "Approved",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Loan {loanId} is not approved.");
            }

            var balance = await _context.LoanBalanceAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.LoanApplicationId == loanId);

            if (balance == null)
            {
                throw new InvalidOperationException(
                    $"Loan {loanId} balance is not initialized.");
            }

            return new LoanBalanceInfo
            {
                LoanId = loan.LoanApplicationId,
                CustomerId = loan.CustomerId,
                Status = loan.Status,
                RemainingPrincipal = balance.RemainingPrincipal,
                RemainingInterest = balance.RemainingInterest
            };
        }

        public async Task<List<EmiBalanceInfo>> GetEmisAsync(
            int loanId)
        {
            return await _context.LoanEmiInstallments
                .AsNoTracking()
                .Where(x => x.LoanApplicationId == loanId)
                .OrderBy(x => x.DueDate)
                .Select(x => new EmiBalanceInfo
                {
                    EmiId = x.EmiId,
                    LoanId = x.LoanApplicationId,
                    PrincipalDue = x.PrincipalDue,
                    InterestDue = x.InterestDue,
                    AmountPaid = x.AmountPaid,
                    DueDate = x.DueDate,
                    Status = x.Status
                })
                .ToListAsync();
        }
    }
}