namespace LoanManagementSystem.Domain.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeCode { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Employee";

        public bool IsActive { get; set; } = true;
    }
}