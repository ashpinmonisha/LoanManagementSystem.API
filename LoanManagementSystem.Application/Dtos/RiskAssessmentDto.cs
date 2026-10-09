namespace LoanManagementSystem.Application.Dtos
{
    public class RiskAssessmentDto
    {
        public decimal MonthlyIncome { get; set; }

        public int EmploymentExperience { get; set; }

        public decimal ExistingLoanAmount { get; set; }
    }
}