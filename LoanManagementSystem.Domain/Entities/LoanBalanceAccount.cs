namespace LoanManagementSystem.Domain.Entities
{
    public class LoanBalanceAccount
    {
        public int LoanApplicationId { get; set; }

        public decimal RemainingPrincipal { get; set; }

        public decimal RemainingInterest { get; set; }
    }
}