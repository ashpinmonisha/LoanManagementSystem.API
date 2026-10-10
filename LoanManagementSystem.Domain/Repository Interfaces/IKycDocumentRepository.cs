
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Domain.RepositoryInterfaces;

public interface IKycDocumentRepository
{
    Task<KycDocument?> GetByIdAsync(int documentId);

    Task<List<KycDocument>> GetByCustomerIdAsync(
        int customerId);

    Task AddAsync(KycDocument document);

    Task SaveChangesAsync();
}
