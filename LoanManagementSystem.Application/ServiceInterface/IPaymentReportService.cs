using LoanManagementSystem.Application.DTOs.Reports;

namespace LoanManagementSystem.Application.Interfaces;

public interface IPaymentReportService
{
    Task<PaymentDashboardDto> GetDashboardAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct);

    Task<List<DailyCollectionDto>> GetDailyAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct);

    Task<List<MonthlyCollectionDto>> GetMonthlyAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct);

    Task<LoanPaymentReportDto> GetLoanAsync(
        int loanId,
        PaymentReportFilterDto filter,
        CancellationToken ct);

    Task<CustomerPaymentReportDto> GetCustomerAsync(
        int customerId,
        PaymentReportFilterDto filter,
        CancellationToken ct);

    Task<RefundReportDto> GetRefundReportAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct);

    Task<List<PaymentReportRowDto>> GetExportRowsAsync(
        PaymentReportFilterDto filter,
        CancellationToken ct);
}