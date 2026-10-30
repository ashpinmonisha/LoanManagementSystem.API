using LoanManagementSystem.Application.Dtos;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface IEmployeeService
    {
        // Employee Login
        Task<object?> LoginAsync(EmployeeLoginDto dto);

        // Create Employee
        Task<int> CreateEmployeeAsync(CreateEmployeeDto dto);

        // Reset Employee Password
        Task<bool> ResetPasswordAsync(
            string employeeCode,
            string newPassword);
    }
}