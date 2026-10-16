
using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;

namespace LoanManagementSystem.Application.Service;

public class KycDocumentService : IKycDocumentService
{
    private readonly IKycDocumentRepository _kycRepository;
    private readonly ICustomerRepository _customerRepository;

    public KycDocumentService(
        IKycDocumentRepository kycRepository,
        ICustomerRepository customerRepository)
    {
        _kycRepository = kycRepository;
        _customerRepository = customerRepository;
    }

    public async Task<object> UploadDocumentAsync(
        KycDocumentUploadDto dto)
    {
        var customer = await _customerRepository
            .GetByIdAsync(dto.CustomerId);

        if (customer == null)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        var document = new KycDocument
        {
            CustomerId = dto.CustomerId,
            DocumentType = dto.DocumentType,
            DocumentNumber = dto.DocumentNumber,
            FilePath = dto.FilePath,
            VerificationStatus = "Pending",
            UploadedAt = DateTime.UtcNow,
            ExpiryDate = dto.ExpiryDate
        };

        await _kycRepository.AddAsync(document);
        await _kycRepository.SaveChangesAsync();

        return new
        {
            message = "KYC document submitted successfully.",
            document.KycDocumentId,
            document.CustomerId,
            document.DocumentType,
            document.VerificationStatus,
            document.UploadedAt
        };
    }

    public async Task<object> GetDocumentsByCustomerIdAsync(
        int customerId)
    {
        var customer = await _customerRepository
            .GetByIdAsync(customerId);

        if (customer == null)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        var documents = await _kycRepository
            .GetByCustomerIdAsync(customerId);

        return documents.Select(document => new
        {
            document.KycDocumentId,
            document.CustomerId,
            document.DocumentType,
            document.VerificationStatus,
            document.UploadedAt,
            document.ExpiryDate,
            document.Remarks
        }).ToList();
    }


    public async Task<object> GetPendingDocumentsAsync()
    {
        var documents = await _kycRepository.GetPendingDocumentsAsync();

        return documents.Select(document => new
        {
            document.KycDocumentId,
            document.CustomerId,
            document.DocumentType,
            document.VerificationStatus,
            document.UploadedAt,
            document.ExpiryDate
        }).ToList();
    }


    public async Task<object> GetDocumentByIdAsync(int documentId)
    {
        var document = await _kycRepository.GetByIdAsync(documentId);

        if (document == null)
        {
            throw new KeyNotFoundException("KYC document not found.");
        }

        return new
        {
            document.KycDocumentId,
            document.CustomerId,
            document.DocumentType,
            document.VerificationStatus,
            document.UploadedAt,
            document.ExpiryDate,
            document.Remarks
        };
    }

    public async Task<object> UpdateVerificationStatusAsync(
        int documentId,
        string status,
        string? remarks)
    {
        var document = await _kycRepository.GetByIdAsync(documentId);

        if (document == null)
        {
            throw new KeyNotFoundException("KYC document not found.");
        }

        var allowedStatuses = new[] { "Pending", "Verified", "Rejected" };

        if (!allowedStatuses.Contains(
            status,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Status must be Pending, Verified, or Rejected.");
        }

        document.VerificationStatus = allowedStatuses.First(
            s => s.Equals(status, StringComparison.OrdinalIgnoreCase));

        document.Remarks = remarks;

        await _kycRepository.SaveChangesAsync();

        return new
        {
            message = "KYC verification status updated.",
            document.KycDocumentId,
            document.VerificationStatus,
            document.Remarks
        };
    }
}
