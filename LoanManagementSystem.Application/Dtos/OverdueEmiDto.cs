namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class OverdueEmiDto
    {
        public int EmiId { get; set; }
        public int LoanId { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OverdueAmount { get; set; }
    }
}