namespace LoanManagementSystem.Application.Dtos
{
    public class LoanDecisionDto
    {
        public bool Approved { get; set; }

        public string Remarks { get; set; } = string.Empty;
    }
}