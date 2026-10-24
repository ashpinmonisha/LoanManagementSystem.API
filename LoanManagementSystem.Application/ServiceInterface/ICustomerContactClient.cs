namespace LoanManagementSystem.Application.ServiceInterface;

public record CustomerContact(
    int CustomerId,
    string? PhoneNumber,
    string? Email,
    bool SmsOptIn,
    bool WhatsAppOptIn,
    bool EmailOptIn);

public interface ICustomerContactClient
{
    Task<CustomerContact?> GetAsync(
        int customerId,
        CancellationToken cancellationToken = default);
}
