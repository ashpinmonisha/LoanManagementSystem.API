namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class EmiBalanceInfo
    {
        public int EmiId { get; set; }

        public int LoanId { get; set; }

        public decimal PrincipalDue { get; set; }

        public decimal InterestDue { get; set; }

        public decimal AmountPaid { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal RemainingAmount =>
            Math.Max(0, PrincipalDue + InterestDue - AmountPaid);
    }
}