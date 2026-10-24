
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Domain.RepositoryInterfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int customerId);

    Task<Customer?> GetByEmailAsync(string email);

    Task<bool> EmailExistsAsync(string email);

    Task<bool> PhoneExistsAsync(string phoneNumber);

    Task AddAsync(Customer customer);

    Task SaveChangesAsync();
}
