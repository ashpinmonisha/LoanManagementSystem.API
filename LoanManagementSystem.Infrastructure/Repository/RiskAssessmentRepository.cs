using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repository
{
    public class RiskAssessmentRepository
        : IRiskAssessmentRepository
    {
        private readonly ApplicationDbContext _context;

        public RiskAssessmentRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            RiskAssessment assessment)
        {
            await _context.RiskAssessments
                .AddAsync(assessment);
        }

        public async Task<RiskAssessment?>
            GetByLoanIdAsync(int loanApplicationId)
        {
            return await _context.RiskAssessments
                .FirstOrDefaultAsync(
                    x => x.LoanApplicationId ==
                         loanApplicationId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}