namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class EmiAllocationResult
    {
        public int EmiId { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool Success { get; set; }
    }
}