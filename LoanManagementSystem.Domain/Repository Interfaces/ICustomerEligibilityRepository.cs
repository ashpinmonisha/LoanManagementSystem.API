
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Domain.RepositoryInterfaces;

public interface ICustomerEligibilityRepository
{
    Task<CustomerEligibilityProfile?> GetByCustomerIdAsync(
        int customerId);

    Task AddAsync(CustomerEligibilityProfile profile);

    Task SaveChangesAsync();
}
