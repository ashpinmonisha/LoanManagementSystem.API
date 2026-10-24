using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace LoanManagementSystem.Infrastructure.Payments
{
    public class RazorpayPaymentService : IRazorpayPaymentService
    {
        private readonly HttpClient _http;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<RazorpayPaymentService> _logger;
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayPaymentService(
            HttpClient http,
            ApplicationDbContext db,
            IConfiguration configuration,
            ILogger<RazorpayPaymentService> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _keyId = configuration["Razorpay:KeyId"]
                ?? throw new InvalidOperationException(
                    "Razorpay KeyId missing");

            _keySecret = configuration["Razorpay:KeySecret"]
                ?? throw new InvalidOperationException(
                    "Razorpay KeySecret missing");

            if (string.IsNullOrWhiteSpace(_keyId) ||
                string.IsNullOrWhiteSpace(_keySecret))
            {
                throw new InvalidOperationException(
                    "Razorpay credentials cannot be empty");
            }

            _http.BaseAddress =
                new Uri("https://api.razorpay.com/v1/");

            var encodedCredentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{_keyId}:{_keySecret}"));

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic", encodedCredentials);
        }

        public async Task<RazorpayOrderResponseDto> CreateOrderAsync(CreateRazorpayOrderDto dto)
        {
            if (dto == null || dto.CustomerId <= 0 || dto.LoanId <= 0 ||
                dto.EmiId <= 0 || dto.Amount <= 0)
                throw new ArgumentException("Customer, loan, EMI, and a positive amount are required.");

            var request = new
            {
                amount = checked((long)Math.Round(dto.Amount * 100m)),
                currency = "INR",
                receipt = $"loan-{dto.LoanId}-emi-{dto.EmiId}-{Guid.NewGuid():N}"
            };

            using var response = await _http.PostAsJsonAsync("orders", request);
            response.EnsureSuccessStatusCode();

            var providerOrder = await response.Content
                .ReadFromJsonAsync<RazorpayOrderResponse>();

            if (providerOrder == null || string.IsNullOrWhiteSpace(providerOrder.Id))
                throw new InvalidOperationException("Razorpay returned an invalid order response.");

            var order = new RazorpayPaymentOrder
            {
                CustomerId = dto.CustomerId,
                LoanId = dto.LoanId,
                EmiId = dto.EmiId,
                Amount = dto.Amount,
                RazorpayOrderId = providerOrder.Id,
                Status = "Created"
            };

            _db.RazorpayPaymentOrders.Add(order);
            await _db.SaveChangesAsync();

            return new RazorpayOrderResponseDto
            {
                KeyId = _keyId,
                OrderId = providerOrder.Id,
                Amount = request.amount,
                Currency = request.currency
            };
        }

        public async Task<PaymentConfirmationDto?> VerifyPaymentAsync(VerifyRazorpayPaymentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.RazorpayOrderId) ||
                string.IsNullOrWhiteSpace(dto.RazorpayPaymentId) ||
                string.IsNullOrWhiteSpace(dto.RazorpaySignature))
                throw new ArgumentException("Razorpay order, payment, and signature are required.");

            var order = await _db.RazorpayPaymentOrders
                .FirstOrDefaultAsync(x => x.RazorpayOrderId == dto.RazorpayOrderId);

            if (order == null)
                return null;

            var payload = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
            var expectedSignature = Convert.ToHexString(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
                .ToLowerInvariant();

            var suppliedSignature = dto.RazorpaySignature.Trim().ToLowerInvariant();
            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expectedSignature),
                    Encoding.UTF8.GetBytes(suppliedSignature)))
                return null;

            if (order.Status == "Completed")
                return await BuildConfirmationAsync(order, dto.RazorpayPaymentId);

            order.RazorpayPaymentId = dto.RazorpayPaymentId;
            order.Status = "Completed";
            order.VerifiedAt = DateTime.UtcNow;

            var payment = await _db.Payments
                .FirstOrDefaultAsync(x => x.PaymentReference == dto.RazorpayPaymentId);

            if (payment == null)
            {
                payment = new Payment
                {
                    CustomerId = order.CustomerId,
                    LoanId = order.LoanId,
                    EmiId = order.EmiId,
                    Amount = order.Amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = "Razorpay",
                    PaymentReference = dto.RazorpayPaymentId,
                    PaymentStatus = "Completed"
                };
                _db.Payments.Add(payment);
            }

            await _db.SaveChangesAsync();
            return await BuildConfirmationAsync(order, dto.RazorpayPaymentId);
        }

        public async Task<PaymentConfirmationDto?> GetConfirmationAsync(string razorpayOrderId)
        {
            if (string.IsNullOrWhiteSpace(razorpayOrderId))
                throw new ArgumentException("Razorpay order ID is required.");

            var order = await _db.RazorpayPaymentOrders
                .FirstOrDefaultAsync(x => x.RazorpayOrderId == razorpayOrderId);

            if (order == null || order.Status != "Completed" ||
                string.IsNullOrWhiteSpace(order.RazorpayPaymentId))
                return null;

            return await BuildConfirmationAsync(order, order.RazorpayPaymentId);
        }

        private async Task<PaymentConfirmationDto?> BuildConfirmationAsync(
            RazorpayPaymentOrder order,
            string paymentReference)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(x => x.PaymentReference == paymentReference);

            if (payment == null)
                return null;

            return new PaymentConfirmationDto
            {
                PaymentId = payment.PaymentId,
                CustomerId = payment.CustomerId,
                LoanId = payment.LoanId,
                EmiId = payment.EmiId ?? order.EmiId,
                Amount = payment.Amount,
                PaymentReference = payment.PaymentReference,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                PaymentDate = payment.PaymentDate,
                ConfirmedAt = order.VerifiedAt ?? payment.CreatedAt
            };
        }

        private sealed class RazorpayOrderResponse
        {
            public string Id { get; set; } = string.Empty;
        }
    }
}