using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
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

        // 1. EMPLOYEE LOGIN
        public async Task<object?> LoginAsync(EmployeeLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.EmployeeCode) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return null;
            }

            var employee = await _employeeRepository
                .GetByEmployeeCodeAsync(dto.EmployeeCode.Trim());

            if (employee == null || !employee.IsActive)
                return null;

            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
                return null;

            bool isValidPassword;

            try
            {
                isValidPassword = BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    employee.PasswordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return null;
            }

            if (!isValidPassword)
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

        // 2. CREATE EMPLOYEE
        public async Task<int> CreateEmployeeAsync(
            CreateEmployeeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.EmployeeCode) ||
                string.IsNullOrWhiteSpace(dto.EmployeeName) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException(
                    "Required employee fields are missing.");
            }

            if (dto.Password.Length < 12)
            {
                throw new ArgumentException(
                    "Password must contain at least 12 characters.");
            }

            string code = dto.EmployeeCode.Trim();

            var existing = await _employeeRepository
                .GetByEmployeeCodeAsync(code);

            if (existing != null)
            {
                throw new InvalidOperationException(
                    "Employee code already exists.");
            }

            var employee = new Employee
            {
                EmployeeName = dto.EmployeeName.Trim(),
                EmployeeCode = code,

                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    dto.Password),

                Role = dto.Role,
                IsActive = dto.IsActive
            };

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return employee.EmployeeId;
        }

        // 3. RESET EMPLOYEE PASSWORD
        public async Task<bool> ResetPasswordAsync(
            string employeeCode,
            string newPassword)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                throw new ArgumentException(
                    "Employee code is required.");
            }

            if (string.IsNullOrWhiteSpace(newPassword) ||
                newPassword.Length < 12)
            {
                throw new ArgumentException(
                    "Password must contain at least 12 characters.");
            }

            var employee = await _employeeRepository
                .GetByEmployeeCodeAsync(employeeCode.Trim());

            if (employee == null)
                return false;

            // Generate a new valid BCrypt password hash
            employee.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _employeeRepository.SaveChangesAsync();

            return true;
        }
    }
}