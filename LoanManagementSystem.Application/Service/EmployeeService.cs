// C#
using BCrypt.Net;
using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using System.Threading.Tasks;

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
            var employee = await _employeeRepository.GetByEmployeeCodeAsync(dto.EmployeeCode);

            if (employee == null || !employee.IsActive)
                return null;

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash))
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

        public async Task<int> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.EmployeeCode) ||
                string.IsNullOrWhiteSpace(dto.EmployeeName) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException("Required employee fields are missing.");
            }

            var existing = await _employeeRepository.GetByEmployeeCodeAsync(dto.EmployeeCode);
            if (existing != null)
                throw new InvalidOperationException("Employee code already exists.");

            var employee = new Employee
            {
                EmployeeName = dto.EmployeeName,
                EmployeeCode = dto.EmployeeCode,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                IsActive = dto.IsActive
            };

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return employee.EmployeeId;
        }
    }
}