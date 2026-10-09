namespace LoanManagementSystem.Domain.Entities
{
    public class LoanApplication
    {
        public int LoanApplicationId { get; set; }

        public int CustomerId { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public decimal LoanAmount { get; set; }

        public int LoanTenure { get; set; }

        public string LoanPurpose { get; set; } = string.Empty;

        public DateTime ApplicationDate { get; set; }

        public string Status { get; set; } = "Pending";

        public string? Remarks { get; set; }

        public int? ReviewedByEmployeeId { get; set; }

        public DateTime? ReviewedDate { get; set; }
    }
}