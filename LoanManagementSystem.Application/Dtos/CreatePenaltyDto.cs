using System.ComponentModel.DataAnnotations;

namespace LoanManagementSystem.Application.DTOs.Payment
{
    public class CreatePenaltyDto
    {
        [Range(1, int.MaxValue)]
        public int LoanId { get; set; }

        [Range(1, int.MaxValue)]
        public int EmiId { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99")]
        public decimal Amount { get; set; }
    }
}