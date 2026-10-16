namespace LoanManagementSystem.Application.Dtos
{
    public class LoanApplicationDto
    {
        public int CustomerId { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public decimal LoanAmount { get; set; }

        public int LoanTenure { get; set; }

        public string LoanPurpose { get; set; } = string.Empty;
    }
}