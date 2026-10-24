using System;

namespace LoanManagementSystem.Application.DTOs.Reports
{
    public class RefundRecordDto
    {
        public int PaymentId { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}