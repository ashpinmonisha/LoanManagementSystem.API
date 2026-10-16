
using LoanManagementSystem.Application.Dtos;

namespace LoanManagementSystem.Application.ServiceInterface;

public interface ICustomerEligibilityService
{
    Task<object> SaveEligibilityAsync(CustomerEligibilityDto dto);

    Task<object> GetEligibilityByCustomerIdAsync(int customerId);

    Task<object> CheckEligibilityAsync(int customerId);
}
