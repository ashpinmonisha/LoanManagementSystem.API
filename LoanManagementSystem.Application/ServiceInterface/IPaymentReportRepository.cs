using LoanManagementSystem.Application.DTOs.Reports;

namespace LoanManagementSystem.Application.Interfaces;

public interface IPaymentReportRepository
{
    Task<List<PaymentReportRowDto>> GetPaymentsAsync(
        DateTime? from,
        DateTime? toExclusive,
        CancellationToken ct);

    Task<List<RefundRecordDto>> GetRefundsAsync(
        DateTime? from,
        DateTime? toExclusive,
        CancellationToken ct);
}