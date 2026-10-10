
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repository;

public class KycDocumentRepository : IKycDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public KycDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<KycDocument?> GetByIdAsync(int documentId)
    {
        return await _context.KycDocuments
            .FirstOrDefaultAsync(
                x => x.KycDocumentId == documentId);
    }

    public async Task<List<KycDocument>> GetByCustomerIdAsync(
        int customerId)
    {
        return await _context.KycDocuments
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync();
    }

    public async Task AddAsync(KycDocument document)
    {
        await _context.KycDocuments.AddAsync(document);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
