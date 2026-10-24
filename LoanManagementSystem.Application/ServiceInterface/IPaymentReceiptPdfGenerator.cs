using LoanManagementSystem.Application.DTOs;

namespace LoanManagementSystem.Application.Interfaces
{
    public interface IPaymentReceiptPdfGenerator
    {
        byte[] Generate(PaymentReceiptDto receipt);
    }
}