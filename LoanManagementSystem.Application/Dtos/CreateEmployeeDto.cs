namespace LoanManagementSystem.Application.Dtos
{
    public class CreateEmployeeDto
    {
        public string EmployeeName { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}