
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.API.Controllers
{
    public class QueueNotificationRequest
    {
        public int PaymentId { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class PaymentNotificationsController : ControllerBase
    {
        private readonly IPaymentNotificationQueue _queue;
        private readonly ApplicationDbContext _db;

        public PaymentNotificationsController(
            IPaymentNotificationQueue queue,
            ApplicationDbContext db)
        {
            _queue = queue;
            _db = db;
        }

        // =====================================
        // 4.6.1 - QUEUE NOTIFICATION
        // =====================================
        [HttpPost("send")]
        public async Task<IActionResult> Send(
            [FromBody] QueueNotificationRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null || request.PaymentId <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid Payment ID is required."
                });
            }

            var payment = await _db.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.PaymentId == request.PaymentId,
                    cancellationToken);

            if (payment == null)
            {
                return NotFound(new
                {
                    message = "Payment not found."
                });
            }

            if (!string.Equals(
                payment.PaymentStatus,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new
                {
                    message = "Payment is not completed."
                });
            }

            // Razorpay payments must be verified
            if (string.Equals(
                payment.PaymentMethod,
                "Razorpay",
                StringComparison.OrdinalIgnoreCase))
            {
                bool verified = await _db.RazorpayPaymentOrders
                    .AsNoTracking()
                    .AnyAsync(o =>
                        o.RazorpayPaymentId ==
                            payment.PaymentReference &&
                        o.CustomerId == payment.CustomerId &&
                        o.LoanId == payment.LoanId &&
                        o.Status == "Completed",
                        cancellationToken);

                if (!verified)
                {
                    return Conflict(new
                    {
                        message = "Razorpay payment is not verified."
                    });
                }
            }

            try
            {
                await _queue.QueueAsync(
                    payment.PaymentId,
                    payment.CustomerId,
                    payment.LoanId,
                    payment.Amount,
                    payment.PaymentReference,
                    cancellationToken);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new
                {
                    message = "Customer contact API request failed."
                });
            }

            // FIXED: No PaymentNotificationId property
            var notifications = await _db.PaymentNotifications
                .AsNoTracking()
                .Where(n => n.PaymentId == payment.PaymentId)
                .Select(n => new
                {
                    n.PaymentId,
                    n.Channel,
                    n.Status
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                paymentId = payment.PaymentId,
                notificationCount = notifications.Count,
                message = notifications.Count > 0
                    ? "Payment notifications recorded."
                    : "No notifications queued. Check customer contact and opt-in settings.",
                notifications
            });
        }

        // =====================================
        // 4.6.5 - NOTIFICATION HISTORY
        // =====================================
        [HttpGet("history/{paymentId:int}")]
        public async Task<IActionResult> GetHistory(
            int paymentId,
            CancellationToken cancellationToken)
        {
            if (paymentId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid Payment ID."
                });
            }

            // FIXED: Uses existing entity properties
            var notifications = await _db.PaymentNotifications
                .AsNoTracking()
                .Where(n => n.PaymentId == paymentId)
                .Select(n => new
                {
                    n.PaymentId,
                    n.CustomerId,
                    n.Channel,
                    n.Destination,
                    n.Subject,
                    n.Message,
                    n.Status
                })
                .ToListAsync(cancellationToken);

            return Ok(notifications);
        }
    }
}
