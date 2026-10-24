using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class ReconcilePaymentDto
    {
        [Required]
        public string PaymentReference { get; set; } = "";

        [Required]
        public string ProviderTransactionId { get; set; } = "";
    }
}