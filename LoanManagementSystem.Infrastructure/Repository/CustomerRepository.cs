
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int customerId)
    {
        return await _context.Customers
            .Include(x => x.EligibilityProfile)
            .FirstOrDefaultAsync(x => x.CustomerId == customerId);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Customers
            .AnyAsync(x => x.Email == email);
    }

    public async Task<bool> PhoneExistsAsync(string phoneNumber)
    {
        return await _context.Customers
            .AnyAsync(x => x.PhoneNumber == phoneNumber);
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
