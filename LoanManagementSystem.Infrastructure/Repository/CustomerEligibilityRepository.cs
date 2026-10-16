
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repository;

public class CustomerEligibilityRepository
    : ICustomerEligibilityRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerEligibilityRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerEligibilityProfile?>
        GetByCustomerIdAsync(int customerId)
    {
        return await _context.CustomerEligibilityProfiles
            .FirstOrDefaultAsync(x => x.CustomerId == customerId);
    }

    public async Task AddAsync(
        CustomerEligibilityProfile profile)
    {
        await _context.CustomerEligibilityProfiles
            .AddAsync(profile);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
