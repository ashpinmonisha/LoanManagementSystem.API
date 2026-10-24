using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Domain.RepositoryInterfaces;

public interface ILoanCalculationRepository
{
    void Add(LoanCalculationRecord record);
    IReadOnlyList<LoanCalculationRecord> GetAll();
    LoanCalculationRecord? GetById(Guid id);
}