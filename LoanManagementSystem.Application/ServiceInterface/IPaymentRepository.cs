using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.ServiceInterface
{
    public interface IPaymentRepository
    {
        // 4.1 Record Payment
        Task<Payment?> GetByReferenceAsync(string reference);

        Task AddAsync(Payment payment);

        Task SaveChangesAsync();

        // 4.2 Payment History
        Task<List<Payment>> GetPaymentHistoryAsync(int loanId);

        Task<List<Payment>> GetCustomerPaymentHistoryAsync(
            int loanId,
            int customerId);


        Task<decimal> GetPenaltyTotalAsync(int loanId);

        Task<Payment?> GetByIdAsync(int paymentId);

        Task<Penalty?> GetPenaltyByEmiAsync(int emiId);

        Task AddPenaltyAsync(Penalty penalty);

        


    }
}