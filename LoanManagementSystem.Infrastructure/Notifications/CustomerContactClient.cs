using LoanManagementSystem.Application.ServiceInterface;
using System.Net;
using System.Net.Http.Json;

namespace LoanManagementSystem.Infrastructure.Notifications
{
    public class CustomerContactClient : ICustomerContactClient
    {
        private readonly HttpClient _http;

        public CustomerContactClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<CustomerContact?> GetAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync(
                $"api/customers/{customerId}/notification-contact",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<CustomerContact>(
                    cancellationToken: cancellationToken);
        }
    }
}