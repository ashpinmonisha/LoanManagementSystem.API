
using LoanManagementSystem.Application.Dtos;

namespace LoanManagementSystem.Application.ServiceInterface;

public interface IKycDocumentService
{
    Task<object> UploadDocumentAsync(KycDocumentUploadDto dto);

    Task<object> GetDocumentsByCustomerIdAsync(int customerId);

    Task<object> GetDocumentByIdAsync(int documentId);


    Task<object> GetPendingDocumentsAsync();


    Task<object> UpdateVerificationStatusAsync(
        int documentId,
        string status,
        string? remarks);
}