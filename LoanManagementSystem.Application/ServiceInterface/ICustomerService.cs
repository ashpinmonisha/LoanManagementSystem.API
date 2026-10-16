
using LoanManagementSystem.Application.Dtos;

namespace LoanManagementSystem.Application.ServiceInterface;

public interface ICustomerService
{
    Task<object?> RegisterAsync(CustomerRegisterDto dto);

    Task<object?> LoginAsync(CustomerLoginDto dto);

    Task<object?> GetProfileAsync(int customerId);

    Task<object?> UpdateProfileAsync(
        int customerId,
        CustomerProfileUpdateDto dto);
}
