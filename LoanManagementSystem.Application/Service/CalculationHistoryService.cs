using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.Mapper;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.RepositoryInterfaces;

namespace LoanManagementSystem.Application.Service;

public sealed class CalculationHistoryService(ILoanCalculationRepository repository) : ICalculationHistoryService
{
    public IReadOnlyList<CalculationHistoryResponse> GetAll() =>
        repository.GetAll().Select(x => x.ToResponse()).ToList();

    public CalculationHistoryResponse GetById(Guid id) =>
        repository.GetById(id)?.ToResponse()
        ?? throw new NotFoundException($"Calculation with id '{id}' was not found.");
}