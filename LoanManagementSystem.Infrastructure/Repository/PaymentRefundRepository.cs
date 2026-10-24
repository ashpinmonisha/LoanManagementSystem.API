using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repositories
{
    public class PaymentRefundRepository : IPaymentRefundRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRefundRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Payment?> GetPaymentAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            return _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.PaymentId == paymentId,
                    ct);
        }

        public Task<PaymentRefund?> GetByIdAsync(
            int refundId,
            CancellationToken ct = default)
        {
            return _context.PaymentRefunds
                .FirstOrDefaultAsync(
                    r => r.RefundId == refundId,
                    ct);
        }

        public Task<List<PaymentRefund>> GetByPaymentAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            return _context.PaymentRefunds
                .AsNoTracking()
                .Where(r => r.PaymentId == paymentId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(ct);
        }

        public async Task<decimal> GetReservedAmountAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            return await _context.PaymentRefunds
                .Where(r =>
                    r.PaymentId == paymentId &&
                    (r.Status == "Pending" ||
                     r.Status == "Approved" ||
                     r.Status == "Processing" ||
                     r.Status == "Completed"))
                .SumAsync(r => (decimal?)r.Amount, ct) ?? 0m;
        }

        public async Task AddAsync(
            PaymentRefund refund,
            CancellationToken ct = default)
        {
            await _context.PaymentRefunds.AddAsync(refund, ct);
            await _context.SaveChangesAsync(ct);
        }

        public Task SaveAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }
    }
}