using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repositories
{
    public class PaymentReconciliationRepository
        : IPaymentReconciliationRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentReconciliationRepository(
            ApplicationDbContext context)
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
                    p => p.PaymentId == paymentId, ct);
        }

        public Task<List<Payment>> GetPaymentsAsync(
            int limit,
            CancellationToken ct = default)
        {
            return _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.PaymentId)
                .Take(limit)
                .ToListAsync(ct);
        }

        public Task<int> CountReferenceAsync(
            string reference,
            CancellationToken ct = default)
        {
            return _context.Payments
                .AsNoTracking()
                .CountAsync(
                    p => p.PaymentReference == reference, ct);
        }

        public async Task AddAsync(
            PaymentReconciliation reconciliation,
            CancellationToken ct = default)
        {
            await _context.PaymentReconciliations
                .AddAsync(reconciliation, ct);

            await _context.SaveChangesAsync(ct);
        }

        public Task<List<PaymentReconciliation>> GetHistoryAsync(
            int paymentId,
            CancellationToken ct = default)
        {
            return _context.PaymentReconciliations
                .AsNoTracking()
                .Where(r => r.PaymentId == paymentId)
                .OrderByDescending(r => r.ReconciledAt)
                .ThenByDescending(r => r.ReconciliationId)
                .ToListAsync(ct);
        }

        public async Task<List<PaymentReconciliation>>
            GetLatestResultsAsync(CancellationToken ct = default)
        {
            // Return only the latest reconciliation per payment.
            // The largest identity ID represents the latest saved run.
            var latestIds = _context.PaymentReconciliations
                .GroupBy(r => r.PaymentId)
                .Select(g => g.Max(r => r.ReconciliationId));

            return await _context.PaymentReconciliations
                .AsNoTracking()
                .Where(r => latestIds.Contains(r.ReconciliationId))
                .OrderByDescending(r => r.ReconciliationId)
                .ToListAsync(ct);
        }

        public async Task<List<PaymentReconciliation>>
            GetLatestMismatchesAsync(CancellationToken ct = default)
        {
            var latest = await GetLatestResultsAsync(ct);

            return latest
                .Where(r => r.Result != "Matched")
                .ToList();
        }
    }
}