using LoanManagementSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LoanManagementSystem.Infrastructure.Payments
{
    public class RazorpayVerificationClient
        : IPaymentGatewayVerificationClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;

        public RazorpayVerificationClient(
            HttpClient http,
            IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<GatewayPaymentInfo?> GetPaymentAsync(
            string razorpayPaymentId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(razorpayPaymentId) ||
                !razorpayPaymentId.StartsWith("pay_"))
            {
                throw new ArgumentException(
                    "A valid Razorpay payment ID is required.");
            }

            var keyId = _configuration["Razorpay:KeyId"];
            var keySecret = _configuration["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(keyId) ||
                string.IsNullOrWhiteSpace(keySecret))
            {
                throw new InvalidOperationException(
                    "Razorpay credentials are missing.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"payments/{Uri.EscapeDataString(razorpayPaymentId)}");

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            using var response = await _http.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            await using var stream =
                await response.Content.ReadAsStreamAsync(ct);

            using var json = await JsonDocument.ParseAsync(
                stream, cancellationToken: ct);

            var root = json.RootElement;

            var id = root.GetProperty("id").GetString() ?? "";
            var amountPaise = root.GetProperty("amount").GetInt64();
            var status = root.GetProperty("status").GetString() ?? "";
            var currency = root.GetProperty("currency").GetString() ?? "";

            if (id != razorpayPaymentId)
                throw new InvalidOperationException(
                    "Razorpay returned a different payment ID.");

            return new GatewayPaymentInfo(
                id,
                amountPaise / 100m,
                status,
                currency);
        }
    }
}