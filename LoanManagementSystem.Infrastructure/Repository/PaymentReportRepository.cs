using LoanManagementSystem.Application.DTOs.Reports;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repositories;

public class PaymentReportRepository : IPaymentReportRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentReportRowDto>> GetPaymentsAsync(
        DateTime? from,
        DateTime? toExclusive,
        CancellationToken ct)
    {
        var query = _context.Payments.AsNoTracking();

        if (from.HasValue)
            query = query.Where(p => p.PaymentDate >= from.Value);

        if (toExclusive.HasValue)
            query = query.Where(p => p.PaymentDate < toExclusive.Value);

        return await query
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentReportRowDto
            {
                PaymentId = p.PaymentId,
                LoanId = p.LoanId,
                CustomerId = p.CustomerId,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentReference = p.PaymentReference,
                PaymentStatus = p.PaymentStatus
            })
            .ToListAsync(ct);
    }

    public async Task<List<RefundRecordDto>> GetRefundsAsync(
        DateTime? from,
        DateTime? toExclusive,
        CancellationToken ct)
    {
        var query = _context.PaymentRefunds.AsNoTracking();

        var start = from ?? DateTime.MinValue;
        var end = toExclusive ?? DateTime.MaxValue;

        query = query.Where(r =>
            (r.RequestedAt >= start && r.RequestedAt < end) ||
            (r.CompletedAt.HasValue &&
             r.CompletedAt.Value >= start &&
             r.CompletedAt.Value < end));

        return await query
            .Select(r => new RefundRecordDto
            {
                PaymentId = r.PaymentId,
                Amount = r.Amount,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                CompletedAt = r.CompletedAt
            })
            .ToListAsync(ct);
    }
}