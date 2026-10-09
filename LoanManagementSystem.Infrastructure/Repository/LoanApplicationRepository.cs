using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repository
{
    public class LoanApplicationRepository
        : ILoanApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public LoanApplicationRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoanApplication?> GetByIdAsync(
            int loanApplicationId)
        {
            return await _context.LoanApplications
                .FirstOrDefaultAsync(
                    x => x.LoanApplicationId ==
                         loanApplicationId);
        }

        public async Task<List<LoanApplication>> GetAllAsync()
        {
            return await _context.LoanApplications
                .OrderByDescending(
                    x => x.ApplicationDate)
                .ToListAsync();
        }

        public async Task AddAsync(
            LoanApplication loanApplication)
        {
            await _context.LoanApplications
                .AddAsync(loanApplication);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}