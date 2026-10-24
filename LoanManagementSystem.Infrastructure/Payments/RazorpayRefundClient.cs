using LoanManagementSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LoanManagementSystem.Infrastructure.Payments
{
    public class RazorpayRefundClient : IRefundGatewayClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public RazorpayRefundClient(
            HttpClient http,
            IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<GatewayRefundResult> CreateRefundAsync(
            string razorpayPaymentId,
            decimal amount,
            string receiptReference,
            CancellationToken ct = default)
        {
            var amountPaise = checked(
                (long)(amount * 100m));

            var payload = new
            {
                amount = amountPaise,
                speed = "normal",
                notes = new
                {
                    internal_refund_reference = receiptReference
                },
                receipt = receiptReference
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"payments/{Uri.EscapeDataString(razorpayPaymentId)}/refund");

            SetAuthentication(request);

            request.Content = JsonContent.Create(payload);

            using var response = await _http.SendAsync(request, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Razorpay refund request failed: HTTP {(int)response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            return ParseResult(body);
        }

        public async Task<GatewayRefundResult> GetRefundAsync(
            string razorpayPaymentId,
            string razorpayRefundId,
            CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"payments/{Uri.EscapeDataString(razorpayPaymentId)}/refunds/{Uri.EscapeDataString(razorpayRefundId)}");

            SetAuthentication(request);

            using var response = await _http.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);

            return ParseResult(body);
        }

        private void SetAuthentication(HttpRequestMessage request)
        {
            var keyId = _config["Razorpay:KeyId"];
            var keySecret = _config["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(keyId) ||
                string.IsNullOrWhiteSpace(keySecret))
            {
                throw new InvalidOperationException(
                    "Razorpay credentials are not configured.");
            }

            var encoded = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", encoded);
        }

        private static GatewayRefundResult ParseResult(string json)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var id = root.GetProperty("id").GetString() ?? "";
            var status = root.GetProperty("status").GetString() ?? "";

            var amountPaise = root.GetProperty("amount").GetInt64();

            return new GatewayRefundResult(
                id,
                status,
                amountPaise / 100m);
        }
    }
}