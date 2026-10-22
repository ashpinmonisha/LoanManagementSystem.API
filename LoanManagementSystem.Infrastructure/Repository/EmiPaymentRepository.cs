using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repositories
{
    public class EmiPaymentRepository : IEmiPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public EmiPaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<PaymentAllocation?> GetAllocationByPaymentIdAsync(
            int paymentId,
            CancellationToken cancellationToken = default)
        {
            return _context.PaymentAllocations
                .FirstOrDefaultAsync(
                    x => x.PaymentId == paymentId,
                    cancellationToken);
        }

        public Task<Payment?> GetCompletedPaymentAsync(
            int paymentId,
            CancellationToken cancellationToken = default)
        {
            return _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.PaymentId == paymentId &&
                         x.PaymentStatus == "Completed",
                    cancellationToken);
        }

        public async Task AddAllocationAsync(
            PaymentAllocation allocation,
            CancellationToken cancellationToken = default)
        {
            await _context.PaymentAllocations.AddAsync(
                allocation, cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}