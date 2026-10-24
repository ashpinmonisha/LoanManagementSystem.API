using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class RecordPaymentDto
    {
        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }

        [Range(1, int.MaxValue)]
        public int LoanId { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        public string PaymentReference { get; set; } = string.Empty;
    }
}