namespace LoanManagementSystem.Application.DTOs.Reconciliation
{
    public class ReconciliationResponseDto
    {
        public int ReconciliationId { get; set; }
        public int PaymentId { get; set; }
        public int LoanId { get; set; }
        public string PaymentReference { get; set; } = "";

        public decimal DatabaseAmount { get; set; }
        public decimal? GatewayAmount { get; set; }

        public string DatabaseStatus { get; set; } = "";
        public string? GatewayStatus { get; set; }

        public string Result { get; set; } = "";
        public string Remarks { get; set; } = "";

        public DateTime ReconciledAt { get; set; }
    }
}