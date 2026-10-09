using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByEmployeeCodeAsync(
            string employeeCode)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(
                    x => x.EmployeeCode == employeeCode);
        }

        public async Task<Employee?> GetByIdAsync(
            int employeeId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(
                    x => x.EmployeeId == employeeId);
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}