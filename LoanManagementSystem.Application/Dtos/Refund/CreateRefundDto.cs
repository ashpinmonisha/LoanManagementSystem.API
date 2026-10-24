using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.DTOs.Refund
{
    public class CreateRefundDto
    {
        [Range(1, int.MaxValue)]
        public int PaymentId { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}