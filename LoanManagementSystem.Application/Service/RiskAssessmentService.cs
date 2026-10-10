using LoanManagementSystem.Application.Dtos;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.RepositoryInterfaces;

namespace LoanManagementSystem.Application.Service
{
    public class RiskAssessmentService
        : IRiskAssessmentService
    {
        private readonly ILoanApplicationRepository
            _loanRepository;

        private readonly IRiskAssessmentRepository
            _riskRepository;

        public RiskAssessmentService(
            ILoanApplicationRepository loanRepository,
            IRiskAssessmentRepository riskRepository)
        {
            _loanRepository = loanRepository;
            _riskRepository = riskRepository;
        }

        public async Task<RiskAssessment?>
            AssessRiskAsync(
                int loanId,
                RiskAssessmentDto dto)
        {
            var loan =
                await _loanRepository.GetByIdAsync(loanId);

            if (loan == null)
            {
                return null;
            }

            decimal score = 0;

            // Monthly income
            if (dto.MonthlyIncome >= 50000)
            {
                score += 30;
            }
            else if (dto.MonthlyIncome >= 30000)
            {
                score += 20;
            }
            else
            {
                score += 10;
            }

            // Loan amount
            if (loan.LoanAmount <=
                dto.MonthlyIncome * 10)
            {
                score += 30;
            }
            else if (loan.LoanAmount <=
                     dto.MonthlyIncome * 20)
            {
                score += 20;
            }
            else
            {
                score += 10;
            }

            // Employment experience
            if (dto.EmploymentExperience >= 5)
            {
                score += 20;
            }
            else if (dto.EmploymentExperience >= 2)
            {
                score += 15;
            }
            else
            {
                score += 10;
            }

            // Existing loan
            if (dto.ExistingLoanAmount == 0)
            {
                score += 20;
            }
            else if (dto.ExistingLoanAmount <=
                     dto.MonthlyIncome * 5)
            {
                score += 10;
            }

            string riskLevel;

            string recommendation;

            if (score >= 80)
            {
                riskLevel = "Low";
                recommendation =
                    "Recommended for approval";
            }
            else if (score >= 60)
            {
                riskLevel = "Medium";
                recommendation =
                    "Manual review required";
            }
            else
            {
                riskLevel = "High";
                recommendation =
                    "Rejection recommended";
            }

            RiskAssessment assessment =
                new RiskAssessment
                {
                    LoanApplicationId = loanId,

                    MonthlyIncome =
                        dto.MonthlyIncome,

                    EmploymentExperience =
                        dto.EmploymentExperience,

                    ExistingLoanAmount =
                        dto.ExistingLoanAmount,

                    RiskScore = score,

                    RiskLevel = riskLevel,

                    Recommendation =
                        recommendation,

                    AssessmentDate =
                        DateTime.UtcNow
                };

            await _riskRepository.AddAsync(assessment);

            await _riskRepository.SaveChangesAsync();

            return assessment;
        }
    }
}