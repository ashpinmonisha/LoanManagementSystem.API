using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.RepositoryInterfaces;

namespace LoanManagementSystem.Application.Service
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly JwtService _jwtService;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            JwtService jwtService)
        {
            _employeeRepository = employeeRepository;
            _jwtService = jwtService;
        }

        public async Task<object?> LoginAsync(EmployeeLoginDto dto)
        {
            var employee =
                await _employeeRepository
                    .GetByEmployeeCodeAsync(dto.EmployeeCode);

            if (employee == null)
                return null;

            if (!employee.IsActive)
                return null;

            if (employee.PasswordHash != dto.Password)
                return null;

            var token = _jwtService.GenerateToken(
                employee.EmployeeId,
                employee.EmployeeCode,
                employee.EmployeeName,
                employee.Role);

            return new
            {
                employee.EmployeeId,
                employee.EmployeeCode,
                employee.EmployeeName,
                employee.Role,
                token
            };
        }
    }
}