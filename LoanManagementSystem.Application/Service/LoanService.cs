using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;

namespace LoanManagementSystem.Application.Service
{
    public class LoanService : ILoanService
    {
        private readonly ILoanApplicationRepository
            _loanRepository;

        public LoanService(
            ILoanApplicationRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<LoanApplication>
            ApplyLoanAsync(
                LoanApplicationDto dto)
        {
            LoanApplication loan =
                new LoanApplication
                {
                    CustomerId = dto.CustomerId,

                    LoanType = dto.LoanType,

                    LoanAmount = dto.LoanAmount,

                    LoanTenure = dto.LoanTenure,

                    LoanPurpose = dto.LoanPurpose,

                    ApplicationDate = DateTime.UtcNow,

                    Status = "Pending"
                };

            await _loanRepository.AddAsync(loan);

            await _loanRepository.SaveChangesAsync();

            return loan;
        }

        public async Task<List<LoanApplication>>
            GetAllLoansAsync()
        {
            return await _loanRepository.GetAllAsync();
        }

        public async Task<LoanApplication?>
            GetLoanByIdAsync(int loanId)
        {
            return await _loanRepository
                .GetByIdAsync(loanId);
        }

        public async Task<LoanApplication?>
            MakeDecisionAsync(
                int loanId,
                LoanDecisionDto dto,
                int employeeId)
        {
            var loan =
                await _loanRepository.GetByIdAsync(loanId);

            if (loan == null)
            {
                return null;
            }

            loan.Status = dto.Approved
                ? "Approved"
                : "Rejected";

            loan.Remarks = dto.Remarks;

            loan.ReviewedByEmployeeId = employeeId;

            loan.ReviewedDate = DateTime.UtcNow;

            await _loanRepository.SaveChangesAsync();

            return loan;
        }
    }
}