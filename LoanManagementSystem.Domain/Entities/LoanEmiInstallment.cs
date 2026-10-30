namespace LoanManagementSystem.Domain.Entities
{
    public class LoanEmiInstallment
    {
        public int EmiId { get; set; }

        public int LoanApplicationId { get; set; }

        public decimal PrincipalDue { get; set; }

        public decimal InterestDue { get; set; }

        public decimal AmountPaid { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; } = "Pending";
    }
}