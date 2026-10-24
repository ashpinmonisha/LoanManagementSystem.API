
using LoanManagementSystem.Infrastructure.Data;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoanManagementSystem.Infrastructure.Notifications;

public class NotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationWorker> _logger;

    public NotificationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(15));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);

                if (!await timer.WaitForNextTickAsync(
                    stoppingToken))
                    break;
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Notification worker failed");

                await Task.Delay(
                    TimeSpan.FromSeconds(15),
                    stoppingToken);
            }
        }
    }

    private async Task ProcessAsync(
        CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var sender = scope.ServiceProvider
            .GetRequiredService<IPaymentNotificationSender>();

        var now = DateTime.UtcNow;

        var pending = await db.PaymentNotifications
            .Where(x =>
                x.Status == "Pending" &&
                x.Attempts < 5 &&
                (x.NextAttemptAt == null ||
                 x.NextAttemptAt <= now))
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        foreach (var notification in pending)
        {
            try
            {
                await sender.SendAsync(
                    notification.Channel,
                    notification.Destination,
                    notification.Subject,
                    notification.Message,
                    ct);

                notification.Status = "Sent";
                notification.SentAt = DateTime.UtcNow;
                notification.LastError = null;
            }
            catch (Exception ex)
            {
                notification.Attempts++;

                notification.LastError =
                    ex.Message.Length > 1000
                        ? ex.Message[..1000]
                        : ex.Message;

                notification.Status =
                    notification.Attempts >= 5
                        ? "Failed"
                        : "Pending";

                notification.NextAttemptAt =
                    DateTime.UtcNow.AddMinutes(
                        Math.Pow(2, notification.Attempts));

                _logger.LogWarning(
                    ex,
                    "Notification {Id} failed",
                    notification.Id);
            }

            await db.SaveChangesAsync(ct);
        }
    }
}