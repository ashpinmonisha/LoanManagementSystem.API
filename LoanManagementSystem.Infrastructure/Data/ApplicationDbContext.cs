using LoanManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<LoanApplication> LoanApplications { get; set; }

        public DbSet<RiskAssessment> RiskAssessments { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // LoanApplication
            modelBuilder.Entity<LoanApplication>()
                .Property(x => x.LoanAmount)
                .HasPrecision(18, 2);

            // RiskAssessment
            modelBuilder.Entity<RiskAssessment>()
                .Property(x => x.MonthlyIncome)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RiskAssessment>()
                .Property(x => x.ExistingLoanAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RiskAssessment>()
                .Property(x => x.RiskScore)
                .HasPrecision(18, 2);
        }
    }
}