using LoanManagementSystem.Application.DTOs.Reports;
using LoanManagementSystem.Application.Interfaces;

namespace LoanManagementSystem.Application.Services;

public class PaymentReportService : IPaymentReportService
{
    private readonly IPaymentReportRepository _repository;

    public PaymentReportService(IPaymentReportRepository repository)
    {
        _repository = repository;
    }

    private static (DateTime? From, DateTime? ToExclusive) Range(
        PaymentReportFilterDto filter)
    {
        if (filter.FromDate.HasValue &&
            filter.ToDate.HasValue &&
            filter.FromDate.Value.Date > filter.ToDate.Value.Date)
        {
            throw new ArgumentException(
                "FromDate cannot be after ToDate.");
        }

        return (
            filter.FromDate?.Date,
            filter.ToDate?.Date.AddDays(1)
        );
    }

    private static bool Completed(string? status) =>
        string.Equals(
            status, "Completed",
            StringComparison.OrdinalIgnoreCase);

    private async Task<List<PaymentReportRowDto>> Payments(
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var range = Range(filter);

        return await _repository.GetPaymentsAsync(
            range.From, range.ToExclusive, ct);
    }

    private async Task<List<RefundRecordDto>> Refunds(
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var range = Range(filter);

        return await _repository.GetRefundsAsync(
            range.From, range.ToExclusive, ct);
    }

    public async Task<PaymentDashboardDto> GetDashboardAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var payments = await Payments(filter, ct);
        var refunds = await Refunds(filter, ct);
        var range = Range(filter);

        var gross = payments
            .Where(p => Completed(p.PaymentStatus))
            .Sum(p => p.Amount);

        var refunded = refunds
            .Where(r =>
                Completed(r.Status) &&
                r.CompletedAt.HasValue &&
                (!range.From.HasValue ||
                 r.CompletedAt.Value >= range.From.Value) &&
                (!range.ToExclusive.HasValue ||
                 r.CompletedAt.Value < range.ToExclusive.Value))
            .Sum(r => r.Amount);

        return new PaymentDashboardDto
        {
            TotalPayments = payments.Count,
            SuccessfulPayments = payments.Count(
                p => Completed(p.PaymentStatus)),
            FailedPayments = payments.Count(
                p => p.PaymentStatus == "Failed"),
            PendingPayments = payments.Count(
                p => p.PaymentStatus == "Pending"),
            GrossCollections = gross,
            CompletedRefunds = refunded,
            NetCollections = gross - refunded
        };
    }

    public async Task<List<DailyCollectionDto>> GetDailyAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var payments = await Payments(filter, ct);

        return payments
            .Where(p => Completed(p.PaymentStatus))
            .GroupBy(p => p.PaymentDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyCollectionDto
            {
                Date = g.Key,
                PaymentCount = g.Count(),
                TotalAmount = g.Sum(p => p.Amount)
            })
            .ToList();
    }

    public async Task<List<MonthlyCollectionDto>> GetMonthlyAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var payments = await Payments(filter, ct);

        return payments
            .Where(p => Completed(p.PaymentStatus))
            .GroupBy(p => new
            {
                p.PaymentDate.Year,
                p.PaymentDate.Month
            })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyCollectionDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                PaymentCount = g.Count(),
                TotalAmount = g.Sum(p => p.Amount)
            })
            .ToList();
    }

    public async Task<LoanPaymentReportDto> GetLoanAsync(
        int loanId,
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var payments = await Payments(filter, ct);

        var selected = payments
            .Where(p => p.LoanId == loanId &&
                        Completed(p.PaymentStatus))
            .ToList();

        return new LoanPaymentReportDto
        {
            LoanId = loanId,
            PaymentCount = selected.Count,
            TotalPaid = selected.Sum(p => p.Amount)
        };
    }

    public async Task<CustomerPaymentReportDto> GetCustomerAsync(
        int customerId,
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var payments = await Payments(filter, ct);

        var selected = payments
            .Where(p => p.CustomerId == customerId &&
                        Completed(p.PaymentStatus))
            .ToList();

        return new CustomerPaymentReportDto
        {
            CustomerId = customerId,
            PaymentCount = selected.Count,
            TotalPaid = selected.Sum(p => p.Amount)
        };
    }

    public async Task<RefundReportDto> GetRefundReportAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var refunds = await Refunds(filter, ct);
        var range = Range(filter);

        var requested = refunds.Where(r =>
            (!range.From.HasValue ||
             r.RequestedAt >= range.From.Value) &&
            (!range.ToExclusive.HasValue ||
             r.RequestedAt < range.ToExclusive.Value))
            .ToList();

        var completed = refunds.Where(r =>
            Completed(r.Status) &&
            r.CompletedAt.HasValue &&
            (!range.From.HasValue ||
             r.CompletedAt.Value >= range.From.Value) &&
            (!range.ToExclusive.HasValue ||
             r.CompletedAt.Value < range.ToExclusive.Value))
            .ToList();

        return new RefundReportDto
        {
            TotalRefunds = requested.Count,
            PendingRefunds = requested.Count(r =>
                r.Status == "Pending" ||
                r.Status == "Approved" ||
                r.Status == "Processing"),
            CompletedRefunds = completed.Count,
            FailedRefunds = requested.Count(
                r => r.Status == "Failed"),
            CompletedRefundAmount = completed.Sum(r => r.Amount)
        };
    }

    public async Task<List<PaymentReportRowDto>> GetExportRowsAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct)
    {
        var payments = await Payments(filter, ct);

        return payments
            .Where(p => Completed(p.PaymentStatus))
            .ToList();
    }
}