namespace LoanManagementSystem.Domain.Entities
{
    public class RiskAssessment
    {
        public int RiskAssessmentId { get; set; }

        public int LoanApplicationId { get; set; }

        public decimal MonthlyIncome { get; set; }

        public int EmploymentExperience { get; set; }

        public decimal ExistingLoanAmount { get; set; }

        public decimal RiskScore { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public string Recommendation { get; set; } = string.Empty;

        public DateTime AssessmentDate { get; set; }
    }
}