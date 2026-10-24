namespace LoanManagementSystem.Application.Interfaces
{
    public record GatewayPaymentInfo(
        string PaymentId,
        decimal Amount,
        string Status,
        string Currency);

    public interface IPaymentGatewayVerificationClient
    {
        Task<GatewayPaymentInfo?> GetPaymentAsync(
            string razorpayPaymentId,
            CancellationToken ct = default);
    }
}