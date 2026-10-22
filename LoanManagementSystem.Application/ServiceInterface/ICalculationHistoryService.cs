using LoanManagementSystem.Application.Dtos;

namespace LoanManagementSystem.Application.ServiceInterface;

public interface ICalculationHistoryService
{
    IReadOnlyList<CalculationHistoryResponse> GetAll();
    CalculationHistoryResponse GetById(Guid id);
}