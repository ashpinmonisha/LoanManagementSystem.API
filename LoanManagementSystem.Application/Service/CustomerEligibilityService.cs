
using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;

namespace LoanManagementSystem.Application.Service;

public class CustomerEligibilityService : ICustomerEligibilityService
{
    private readonly ICustomerEligibilityRepository _eligibilityRepository;
    private readonly ICustomerRepository _customerRepository;

    public CustomerEligibilityService(
        ICustomerEligibilityRepository eligibilityRepository,
        ICustomerRepository customerRepository)
    {
        _eligibilityRepository = eligibilityRepository;
        _customerRepository = customerRepository;
    }

    public async Task<object> SaveEligibilityAsync(
        CustomerEligibilityDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);

        if (customer == null)
            throw new KeyNotFoundException("Customer not found.");

        if (dto.MonthlyIncome <= 0)
            throw new ArgumentException("Monthly income must be greater than zero.");

        if (dto.ExistingMonthlyObligations > dto.MonthlyIncome)
            throw new ArgumentException(
                "Existing monthly obligations cannot exceed monthly income.");

        var existing = await _eligibilityRepository
            .GetByCustomerIdAsync(dto.CustomerId);

        if (existing != null)
        {
            existing.MonthlyIncome = dto.MonthlyIncome;
            existing.EmploymentType = dto.EmploymentType;
            existing.CreditScore = dto.CreditScore;
            existing.ExistingLoanCount = dto.ExistingLoanCount;
            existing.ExistingMonthlyObligations =
                dto.ExistingMonthlyObligations;

            await _eligibilityRepository.SaveChangesAsync();

            return new
            {
                message = "Eligibility details updated successfully.",
                existing.CustomerId,
                existing.MonthlyIncome,
                existing.EmploymentType,
                existing.CreditScore,
                existing.ExistingLoanCount,
                existing.ExistingMonthlyObligations
            };
        }

        var profile = new CustomerEligibilityProfile
        {
            CustomerId = dto.CustomerId,
            MonthlyIncome = dto.MonthlyIncome,
            EmploymentType = dto.EmploymentType,
            CreditScore = dto.CreditScore,
            ExistingLoanCount = dto.ExistingLoanCount,
            ExistingMonthlyObligations = dto.ExistingMonthlyObligations
        };

        await _eligibilityRepository.AddAsync(profile);
        await _eligibilityRepository.SaveChangesAsync();

        return new
        {
            message = "Eligibility details saved successfully.",
            profile.CustomerId,
            profile.MonthlyIncome,
            profile.EmploymentType,
            profile.CreditScore,
            profile.ExistingLoanCount,
            profile.ExistingMonthlyObligations
        };
    }

    public async Task<object> GetEligibilityByCustomerIdAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);

        if (customer == null)
            throw new KeyNotFoundException("Customer not found.");

        var profile = await _eligibilityRepository
            .GetByCustomerIdAsync(customerId);

        if (profile == null)
            throw new KeyNotFoundException(
                "Eligibility details not found for this customer.");

        return new
        {
            profile.CustomerId,
            profile.MonthlyIncome,
            profile.EmploymentType,
            profile.CreditScore,
            profile.ExistingLoanCount,
            profile.ExistingMonthlyObligations
        };
    }

    public async Task<object> CheckEligibilityAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);

        if (customer == null)
            throw new KeyNotFoundException("Customer not found.");

        var profile = await _eligibilityRepository
            .GetByCustomerIdAsync(customerId);

        if (profile == null)
            throw new KeyNotFoundException(
                "Please submit eligibility details first.");

        decimal disposableIncome =
            profile.MonthlyIncome - profile.ExistingMonthlyObligations;

        bool eligible =
            profile.MonthlyIncome >= 15000 &&
            profile.CreditScore >= 650 &&
            disposableIncome > 0;

        return new
        {
            customerId,
            eligible,
            monthlyIncome = profile.MonthlyIncome,
            creditScore = profile.CreditScore,
            disposableIncome,
            message = eligible
                ? "Customer meets the basic demo eligibility criteria."
                : "Customer does not meet the basic demo eligibility criteria."
        };
    }
}
