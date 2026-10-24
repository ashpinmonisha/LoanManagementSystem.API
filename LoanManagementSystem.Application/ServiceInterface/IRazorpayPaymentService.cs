using LoanManagementSystem.Application.Dtos;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface IRazorpayPaymentService
    {
        Task<RazorpayOrderResponseDto> CreateOrderAsync(
            CreateRazorpayOrderDto dto);

        Task<PaymentConfirmationDto?> VerifyPaymentAsync(
            VerifyRazorpayPaymentDto dto);

        Task<PaymentConfirmationDto?> GetConfirmationAsync(
            string razorpayOrderId);
    }
}