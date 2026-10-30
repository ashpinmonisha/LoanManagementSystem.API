using System.Net;
using System.Net.Http.Json;
using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;
using Microsoft.Extensions.Logging;

namespace LoanManagementSystem.Infrastructure.Clients
{
    public class LoanModuleClient : ILoanModuleClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LoanModuleClient> _logger;

        public LoanModuleClient(
            HttpClient httpClient,
            ILogger<LoanModuleClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // MODULE 4.3 - GET LOAN BALANCE
        public async Task<LoanBalanceInfo?> GetLoanBalanceAsync(
            int loanId)
        {
            if (loanId <= 0)
                throw new ArgumentException("Invalid Loan ID.");

            // Correct route for LoanController
            string url = $"api/Loan/{loanId}/balance";

            _logger.LogInformation(
                "Requesting loan balance from {BaseUrl}{Path}",
                _httpClient.BaseAddress,
                url);

            using var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning(
                    "Loan balance endpoint returned 404 for LoanId {LoanId}. " +
                    "Verify the endpoint exists and the loan is initialized.",
                    loanId);

                return null;
            }

            response.EnsureSuccessStatusCode();

            var loan = await response.Content
                .ReadFromJsonAsync<LoanBalanceInfo>();

            return loan ?? throw new InvalidOperationException(
                "Loan Balance API returned an empty response.");
        }

        // GET EMI SCHEDULE
        public async Task<List<EmiBalanceInfo>> GetEmisAsync(
            int loanId)
        {
            // Verify this route against the actual EMI controller.
            string url = $"api/emis/loan/{loanId}";

            using var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<EmiBalanceInfo>>()
                ?? throw new InvalidOperationException(
                    "EMI Module returned an invalid response.");
        }

        // ALLOCATE EMI PAYMENT
        public async Task<EmiAllocationResult> AllocatePaymentAsync(
            int emiId,
            decimal amount,
            string idempotencyKey)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                $"api/emis/{emiId}/allocate",
                new
                {
                    Amount = amount,
                    IdempotencyKey = idempotencyKey
                });

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<EmiAllocationResult>()
                ?? throw new InvalidOperationException(
                    "Invalid EMI allocation response.");
        }

        // CLOSE LOAN
        public async Task<bool> CloseLoanAsync(int loanId)
        {
            using var response = await _httpClient.PostAsync(
                $"api/loans/{loanId}/close",
                null);

            response.EnsureSuccessStatusCode();

            return true;
        }
    }
}
