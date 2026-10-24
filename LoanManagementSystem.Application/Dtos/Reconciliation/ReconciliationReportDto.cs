namespace LoanManagementSystem.Application.DTOs.Reconciliation
{
    public class ReconciliationReportDto
    {
        public int TotalPaymentsChecked { get; set; }
        public int Matched { get; set; }
        public int AmountMismatches { get; set; }
        public int StatusMismatches { get; set; }
        public int DuplicateReferences { get; set; }
        public int Pending { get; set; }
        public int GatewayNotFound { get; set; }
        public int InvalidReferences { get; set; }
    }
}