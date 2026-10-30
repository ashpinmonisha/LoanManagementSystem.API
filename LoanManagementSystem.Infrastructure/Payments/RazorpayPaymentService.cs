
using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

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

            // Load credentials from configuration / User Secrets
            _keyId = (configuration["Razorpay:KeyId"] ?? "").Trim();
            _keySecret = (configuration["Razorpay:KeySecret"] ?? "").Trim();

            // Validate test-mode configuration
            if (string.IsNullOrWhiteSpace(_keyId) ||
                string.IsNullOrWhiteSpace(_keySecret) ||
                !_keyId.StartsWith("rzp_test_", StringComparison.Ordinal) ||
                _keyId.Contains("YOUR_", StringComparison.OrdinalIgnoreCase) ||
                _keySecret.Contains("YOUR_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Valid Razorpay Test Key ID and Key Secret are not configured.");
            }

            _http.BaseAddress = new Uri(
                "https://api.razorpay.com/v1/");

            _http.Timeout = TimeSpan.FromSeconds(30);

            // Razorpay requires HTTP Basic authentication
            string credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic", credentials);

            _logger.LogInformation(
                "Razorpay test configuration initialized.");
        }

        // ===========================================
        // 4.5.1 - CREATE RAZORPAY ORDER
        // ===========================================
        public async Task<RazorpayOrderResponseDto> CreateOrderAsync(
            CreateRazorpayOrderDto dto)
        {
            if (dto == null ||
                dto.CustomerId <= 0 ||
                dto.LoanId <= 0 ||
                dto.EmiId <= 0)
            {
                throw new ArgumentException(
                    "Valid Customer ID, Loan ID and EMI ID are required.");
            }

            if (dto.Amount < 1m ||
                decimal.Round(dto.Amount, 2) != dto.Amount)
            {
                throw new ArgumentException(
                    "Amount must be at least INR 1 with at most two decimal places.");
            }

            // INR to paise
            long amountPaise = checked(
                (long)(dto.Amount * 100m));

            var request = new
            {
                amount = amountPaise,
                currency = "INR",

                // Maximum 40 characters
                receipt = $"lms_{Guid.NewGuid():N}"
            };

            // Call Razorpay Orders API
            using var response = await _http.PostAsJsonAsync(
                "orders",
                request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Razorpay order creation failed. HTTP {StatusCode}",
                    (int)response.StatusCode);

                throw new HttpRequestException(
                    $"Razorpay order creation failed with HTTP {(int)response.StatusCode}.",
                    null,
                    response.StatusCode);
            }

            var providerOrder = await response.Content
                .ReadFromJsonAsync<RazorpayOrderResponse>();

            if (providerOrder == null ||
                string.IsNullOrWhiteSpace(providerOrder.Id))
            {
                throw new InvalidOperationException(
                    "Razorpay returned an invalid order response.");
            }

            // Save order in SQL Server
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

            _logger.LogInformation(
                "Razorpay order created and saved. OrderId: {OrderId}",
                providerOrder.Id);

            return new RazorpayOrderResponseDto
            {
                KeyId = _keyId,
                OrderId = providerOrder.Id,
                Amount = amountPaise,
                Currency = "INR"
            };
        }

        // ===========================================
        // 4.5.2 - VERIFY RAZORPAY PAYMENT
        // ===========================================
        public async Task<PaymentConfirmationDto?> VerifyPaymentAsync(
            VerifyRazorpayPaymentDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.RazorpayOrderId) ||
                string.IsNullOrWhiteSpace(dto.RazorpayPaymentId) ||
                string.IsNullOrWhiteSpace(dto.RazorpaySignature))
            {
                throw new ArgumentException(
                    "Order ID, Payment ID and Signature are required.");
            }

            string orderId = dto.RazorpayOrderId.Trim();
            string paymentId = dto.RazorpayPaymentId.Trim();

            // STEP 1 - Find stored order
            var order = await _db.RazorpayPaymentOrders
                .FirstOrDefaultAsync(x =>
                    x.RazorpayOrderId == orderId);

            if (order == null)
                return null;

            // STEP 2 - Compute server-side signature
            string payload =
                $"{order.RazorpayOrderId}|{paymentId}";

            byte[] expectedSignature;

            using (var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(_keySecret)))
            {
                expectedSignature = hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(payload));
            }

            byte[] suppliedSignature;

            try
            {
                suppliedSignature = Convert.FromHexString(
                    dto.RazorpaySignature.Trim());
            }
            catch (FormatException)
            {
                return null;
            }

            bool signatureValid =
                CryptographicOperations.FixedTimeEquals(
                    expectedSignature,
                    suppliedSignature);

            if (!signatureValid)
                return null;

            // STEP 3 - Idempotent completed order
            if (string.Equals(
                order.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                if (order.RazorpayPaymentId != paymentId)
                    return null;

                return await BuildConfirmationAsync(
                    order,
                    paymentId);
            }

            // STEP 4 - Fetch actual payment from Razorpay
            using var response = await _http.GetAsync(
                $"payments/{Uri.EscapeDataString(paymentId)}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Razorpay payment lookup failed. HTTP {StatusCode}",
                    (int)response.StatusCode);

                return null;
            }

            var providerPayment = await response.Content
                .ReadFromJsonAsync<RazorpayPaymentResponse>();

            if (providerPayment == null)
                return null;

            // STEP 5 - Verify provider payment details
            long expectedAmount = checked(
                (long)(order.Amount * 100m));

            bool isValid =
                providerPayment.Id == paymentId &&
                providerPayment.OrderId == order.RazorpayOrderId &&
                providerPayment.Amount == expectedAmount &&
                providerPayment.Currency == "INR" &&
                providerPayment.Status == "captured" &&
                providerPayment.Captured;

            if (!isValid)
                return null;

            // STEP 6 - Prevent payment ID reuse
            bool usedElsewhere =
                await _db.RazorpayPaymentOrders.AnyAsync(x =>
                    x.RazorpayPaymentId == paymentId &&
                    x.RazorpayOrderId != order.RazorpayOrderId);

            if (usedElsewhere)
                return null;

            // STEP 7 - Find existing payment
            var payment = await _db.Payments
                .FirstOrDefaultAsync(x =>
                    x.PaymentReference == paymentId);

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
                    PaymentReference = paymentId,
                    PaymentStatus = "Completed"
                };

                _db.Payments.Add(payment);
            }
            else
            {
                // Do not accept mismatched existing records
                if (payment.CustomerId != order.CustomerId ||
                    payment.LoanId != order.LoanId ||
                    payment.EmiId != order.EmiId ||
                    payment.Amount != order.Amount ||
                    !string.Equals(
                        payment.PaymentMethod,
                        "Razorpay",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                payment.PaymentStatus = "Completed";
            }

            // STEP 8 - Complete stored order
            order.RazorpayPaymentId = paymentId;
            order.Status = "Completed";
            order.VerifiedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Razorpay payment confirmed for OrderId: {OrderId}",
                order.RazorpayOrderId);

            return await BuildConfirmationAsync(
                order,
                paymentId);
        }

        // ===========================================
        // 4.5.3 - GET PAYMENT CONFIRMATION
        // ===========================================
        public async Task<PaymentConfirmationDto?> GetConfirmationAsync(
            string razorpayOrderId)
        {
            if (string.IsNullOrWhiteSpace(razorpayOrderId))
            {
                throw new ArgumentException(
                    "Razorpay Order ID is required.");
            }

            var order = await _db.RazorpayPaymentOrders
                .FirstOrDefaultAsync(x =>
                    x.RazorpayOrderId == razorpayOrderId);

            if (order == null ||
                !string.Equals(
                    order.Status,
                    "Completed",
                    StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(order.RazorpayPaymentId))
            {
                return null;
            }

            return await BuildConfirmationAsync(
                order,
                order.RazorpayPaymentId);
        }

        // ===========================================
        // BUILD PAYMENT CONFIRMATION
        // ===========================================
        private async Task<PaymentConfirmationDto?> BuildConfirmationAsync(
            RazorpayPaymentOrder order,
            string paymentReference)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(x =>
                    x.PaymentReference == paymentReference);

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

        // ===========================================
        // RAZORPAY RESPONSE MODELS
        // ===========================================
        private sealed class RazorpayOrderResponse
        {
            public string Id { get; set; } = string.Empty;
        }

        private sealed class RazorpayPaymentResponse
        {
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("order_id")]
            public string? OrderId { get; set; }

            public long Amount { get; set; }

            public string Currency { get; set; } = string.Empty;

            public string Status { get; set; } = string.Empty;

            public bool Captured { get; set; }
        }
    }
}
