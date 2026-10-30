namespace LoanManagementSystem.Application.Dtos
{
    public class ResetEmployeePasswordDto
    {
        public string EmployeeCode { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}