
using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using Microsoft.AspNetCore.Identity;

namespace LoanManagementSystem.Application.Service;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly JwtService _jwtService;
    private readonly PasswordHasher<Customer> _passwordHasher;

    public CustomerService(
        ICustomerRepository customerRepository,
        JwtService jwtService)
    {
        _customerRepository = customerRepository;
        _jwtService = jwtService;
        _passwordHasher = new PasswordHasher<Customer>();
    }

    public async Task<object?> RegisterAsync(CustomerRegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var phone = dto.PhoneNumber.Trim();

        if (dto.DateOfBirth.HasValue &&
            dto.DateOfBirth.Value.Date >= DateTime.UtcNow.Date)
        {
            return new { Message = "Date of birth must be in the past." };
        }

        if (await _customerRepository.EmailExistsAsync(email))
            return new { Message = "Email already registered." };

        if (await _customerRepository.PhoneExistsAsync(phone))
            return new { Message = "Phone number already registered." };

        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PhoneNumber = phone,
            DateOfBirth = dto.DateOfBirth,
            Gender = string.IsNullOrWhiteSpace(dto.Gender)
                ? null
                : dto.Gender.Trim()
        };

        customer.PasswordHash = _passwordHasher.HashPassword(
            customer, dto.Password);

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();

        return new
        {
            Message = "Customer registered successfully.",
            customer.CustomerId,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber
        };
    }

    public async Task<object?> LoginAsync(CustomerLoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var customer = await _customerRepository.GetByEmailAsync(email);

        if (customer == null || !customer.IsActive)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(
            customer, customer.PasswordHash, dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        var token = _jwtService.GenerateCustomerToken(
            customer.CustomerId, customer.Email);

        return new
        {
            Message = "Login successful.",
            customer.CustomerId,
            customer.FullName,
            customer.Email,
            Token = token
        };
    }

    public async Task<object?> GetProfileAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);

        if (customer == null)
            return null;

        return new
        {
            customer.CustomerId,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.DateOfBirth,
            customer.Gender,
            customer.CreatedAt,
            customer.IsActive
        };
    }

    public async Task<object?> UpdateProfileAsync(
        int customerId,
        CustomerProfileUpdateDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);

        if (customer == null)
            return null;

        if (dto.DateOfBirth.HasValue &&
            dto.DateOfBirth.Value.Date >= DateTime.UtcNow.Date)
        {
            return new { Message = "Date of birth must be in the past." };
        }

        var phone = dto.PhoneNumber.Trim();

        if (!string.Equals(
                customer.PhoneNumber,
                phone,
                StringComparison.OrdinalIgnoreCase) &&
            await _customerRepository.PhoneExistsAsync(phone))
        {
            return new { Message = "Phone number already registered." };
        }

        customer.FullName = dto.FullName.Trim();
        customer.PhoneNumber = phone;
        customer.DateOfBirth = dto.DateOfBirth;
        customer.Gender = string.IsNullOrWhiteSpace(dto.Gender)
            ? null
            : dto.Gender.Trim();

        await _customerRepository.SaveChangesAsync();

        return new
        {
            Message = "Profile updated successfully.",
            customer.CustomerId,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.DateOfBirth,
            customer.Gender
        };
    }
}
