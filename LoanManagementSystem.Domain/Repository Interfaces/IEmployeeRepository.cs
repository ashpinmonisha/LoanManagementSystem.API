using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Domain.RepositoryInterfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByEmployeeCodeAsync(
            string employeeCode);

        Task<Employee?> GetByIdAsync(
            int employeeId);

        Task AddAsync(Employee employee);

        Task SaveChangesAsync();
    }
}