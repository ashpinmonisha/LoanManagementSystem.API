namespace LoanManagementSystem.Application.ServiceInterface;

public interface IPaymentNotificationQueue
{
    Task QueueAsync(
        int paymentId,
        int customerId,
        int loanId,
        decimal amount,
        string reference,
        CancellationToken cancellationToken = default);
}