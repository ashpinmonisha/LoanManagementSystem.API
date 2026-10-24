using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repositories
{
    public class PaymentReceiptRepository : IPaymentReceiptRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentReceiptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetCompletedPaymentAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.PaymentId == paymentId &&
                         p.PaymentStatus == "Completed",
                    ct);
        }

        public async Task<PaymentReceipt?> GetByPaymentIdAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            return await _context.PaymentReceipts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.PaymentId == paymentId,
                    ct);
        }

        public async Task<List<PaymentReceipt>> GetByLoanIdAsync(
            int loanId,
            CancellationToken ct = default)
        {
            return await _context.PaymentReceipts
                .AsNoTracking()
                .Where(r => r.LoanId == loanId)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync(ct);
        }

        public async Task AddAsync(
            PaymentReceipt receipt,
            CancellationToken ct = default)
        {
            _context.PaymentReceipts.Add(receipt);
            await _context.SaveChangesAsync(ct);
        }
    }
}