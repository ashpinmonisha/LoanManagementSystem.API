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


        public DbSet<Customer> Customers { get; set; }

        public DbSet<KycDocument> KycDocuments { get; set; }

        public DbSet<CustomerEligibilityProfile>
            CustomerEligibilityProfiles
        { get; set; }



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



            modelBuilder.Entity<Customer>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(x => x.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasMany(x => x.KycDocuments)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasOne(x => x.EligibilityProfile)
                .WithOne(x => x.Customer)
                .HasForeignKey<CustomerEligibilityProfile>(
                    x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerEligibilityProfile>()
                .Property(x => x.MonthlyIncome)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CustomerEligibilityProfile>()
                .Property(x => x.ExistingLoanAmount)
                .HasPrecision(18, 2);

        }
    }
}