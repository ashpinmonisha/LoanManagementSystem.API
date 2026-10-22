
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Infrastructure.Data;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Application.ServiceInterface;

namespace LoanManagementSystem.Infrastructure.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // 4.1 Record Payment
        public async Task<Payment?> GetByReferenceAsync(
            string reference)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(
                    p => p.PaymentReference == reference);
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // 4.2 Payment History
        public async Task<List<Payment>> GetPaymentHistoryAsync(
            int loanId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.LoanId == loanId)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetCustomerPaymentHistoryAsync(
            int loanId,
            int customerId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p =>
                    p.LoanId == loanId &&
                    p.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .ToListAsync();
        }

        public async Task<decimal> GetPenaltyTotalAsync(int loanId)
        {
            var penalties = await _context.Penalties
                .Where(p => p.LoanId == loanId)
                .ToListAsync();

            return penalties.Sum(p =>
                Math.Max(0m, p.Amount - p.PaidAmount));
        }

        public async Task<Payment?> GetByIdAsync(int paymentId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        public async Task<Penalty?> GetPenaltyByEmiAsync(int emiId)
        {
            return await _context.Penalties
                .FirstOrDefaultAsync(p => p.EmiId == emiId);
        }

        public async Task AddPenaltyAsync(Penalty penalty)
        {
            await _context.Penalties.AddAsync(penalty);
        }
    }
}