namespace LoanManagementSystem.Application.Interfaces
{
    public record GatewayRefundResult(
        string RefundId,
        string Status,
        decimal Amount);

    public interface IRefundGatewayClient
    {
        Task<GatewayRefundResult> CreateRefundAsync(
            string razorpayPaymentId,
            decimal amount,
            string receiptReference,
            CancellationToken ct = default);

        Task<GatewayRefundResult> GetRefundAsync(
            string razorpayPaymentId,
            string razorpayRefundId,
            CancellationToken ct = default);
    }
}