
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Domain.RepositoryInterfaces;

public interface IKycDocumentRepository
{
    Task<KycDocument?> GetByIdAsync(int documentId);

    Task<IEnumerable<KycDocument>> GetByCustomerIdAsync(int customerId);

    Task<IEnumerable<KycDocument>> GetPendingDocumentsAsync();

    Task AddAsync(KycDocument document);

    Task SaveChangesAsync();
}
