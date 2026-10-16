using LoanManagementSystem.Application.Dtos;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface IEmployeeService
    {
        Task<object?> LoginAsync(
            EmployeeLoginDto dto);
    }
}