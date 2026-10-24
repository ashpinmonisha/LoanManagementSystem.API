using System.Net;
using System.Net.Http.Json;
using LoanManagementSystem.Application.DTOs.Payment;
using LoanManagementSystem.Application.ServiceInterface;

namespace LoanManagementSystem.Infrastructure.Clients
{
    public class LoanModuleClient : ILoanModuleClient
    {
        private readonly HttpClient _httpClient;

        public LoanModuleClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoanBalanceInfo?> GetLoanBalanceAsync(
            int loanId)
        {
            var response = await _httpClient.GetAsync(
                $"api/loans/{loanId}/balance");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<LoanBalanceInfo>();
        }

        public async Task<List<EmiBalanceInfo>> GetEmisAsync(
            int loanId)
        {
            var response = await _httpClient.GetAsync(
                $"api/emis/loan/{loanId}");

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<EmiBalanceInfo>>()
                ?? new List<EmiBalanceInfo>();
        }
        public async Task<EmiAllocationResult> AllocatePaymentAsync(
    int emiId,
    decimal amount,
    string idempotencyKey)
        {
            var response = await _httpClient.PostAsJsonAsync(
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
                    "Invalid EMI allocation response");
        }

        public async Task<bool> CloseLoanAsync(int loanId)
        {
            var response = await _httpClient.PostAsync(
                $"api/loans/{loanId}/close",
                null);

            response.EnsureSuccessStatusCode();

            return true;
        }
    }
}