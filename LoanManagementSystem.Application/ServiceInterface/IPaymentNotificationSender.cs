

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface IPaymentNotificationSender
    {
        Task SendAsync(
            string channel,
            string destination,
            string subject,
            string message,
            CancellationToken cancellationToken = default);
    }
}