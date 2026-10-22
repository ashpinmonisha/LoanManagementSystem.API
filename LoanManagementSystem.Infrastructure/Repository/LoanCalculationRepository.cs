using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Repository;

public sealed class LoanCalculationRepository(ApplicationDbContext context) : ILoanCalculationRepository
{
    public void Add(LoanCalculationRecord record)
    {
        context.LoanCalculations.Add(record);
        context.SaveChanges();
    }

    public IReadOnlyList<LoanCalculationRecord> GetAll() =>
        context.LoanCalculations
            .AsNoTracking()
            .OrderByDescending(x => x.CalculatedAtUtc)
            .ToList();

    public LoanCalculationRecord? GetById(Guid id) =>
        context.LoanCalculations
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id);
}