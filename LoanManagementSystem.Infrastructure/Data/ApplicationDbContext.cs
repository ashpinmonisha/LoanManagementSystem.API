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

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Penalty> Penalties { get; set; }

        public DbSet<PaymentNotification> PaymentNotifications { get; set; }

        public DbSet<RiskAssessment> RiskAssessments { get; set; }

        // MODULE 3: EMI & Interest
        public DbSet<LoanCalculationRecord> LoanCalculations { get; set; }

        public DbSet<PaymentReceipt> PaymentReceipts { get; set; }


        public DbSet<PaymentRefund> PaymentRefunds { get; set; }


        public DbSet<PaymentReconciliation> PaymentReconciliations
        { get; set; }


        public DbSet<RazorpayPaymentOrder> RazorpayPaymentOrders
    => Set<RazorpayPaymentOrder>();

        public DbSet<EmiPaymentAllocation> EmiPaymentAllocations
    => Set<EmiPaymentAllocation>();

        public DbSet<PaymentAllocation> PaymentAllocations
    => Set<PaymentAllocation>();

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

            modelBuilder.Entity<CustomerEligibilityProfile>()
                .Property(x => x.ExistingMonthlyObligations)
                .HasPrecision(18, 2);

            // MODULE 3: EMI & Interest
            modelBuilder.Entity<LoanCalculationRecord>(entity =>
            {
                entity.ToTable("LoanCalculations");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.CalculationType)
                      .HasConversion<string>()
                      .HasMaxLength(30);

                entity.Property(x => x.LoanAmount).HasPrecision(18, 2);
                entity.Property(x => x.InterestRate).HasPrecision(5, 2);
                entity.Property(x => x.MonthlyEmi).HasPrecision(18, 2);
                entity.Property(x => x.TotalInterest).HasPrecision(18, 2);
                entity.Property(x => x.TotalRepayment).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.PaymentId);

                entity.Property(p => p.Amount)
                      .HasPrecision(18, 2);

                entity.Property(p => p.PaymentMethod)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(p => p.PaymentReference)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(p => p.PaymentStatus)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.HasIndex(p => p.PaymentReference)
                      .IsUnique();
            });

            modelBuilder.Entity<Penalty>(entity =>
            {
                entity.HasKey(p => p.PenaltyId);

                entity.Property(p => p.Amount)
                      .HasPrecision(18, 2);

                entity.Property(p => p.PaidAmount)
                      .HasPrecision(18, 2);

                entity.HasIndex(p => p.EmiId)
                      .IsUnique();
            });

            modelBuilder.Entity<PaymentNotification>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Channel)
                    .HasMaxLength(20).IsRequired();

                entity.Property(x => x.Destination)
                    .HasMaxLength(320).IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(20).IsRequired();

                entity.HasIndex(x => new
                {
                    x.PaymentId,
                    x.Channel
                }).IsUnique();
            });
            modelBuilder.Entity<RazorpayPaymentOrder>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.RazorpayOrderId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.RazorpayPaymentId)
                    .HasMaxLength(100);

                entity.HasIndex(x => x.RazorpayOrderId)
                    .IsUnique();

                entity.HasIndex(x => x.RazorpayPaymentId)
                    .IsUnique()
                    .HasFilter("[RazorpayPaymentId] IS NOT NULL");
            });

            modelBuilder.Entity<EmiPaymentAllocation>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.IdempotencyKey)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.HasIndex(x => x.PaymentId)
                    .IsUnique();

                entity.HasIndex(x => x.IdempotencyKey)
                    .IsUnique();
            });

            modelBuilder.Entity<PaymentAllocation>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.IdempotencyKey)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.HasIndex(x => x.PaymentId)
                    .IsUnique();

                entity.HasIndex(x => x.IdempotencyKey)
                    .IsUnique();
            });
            modelBuilder.Entity<PaymentReceipt>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.ReceiptNumber)
                    .HasMaxLength(80)
                    .IsRequired();

                entity.HasIndex(r => r.ReceiptNumber)
                    .IsUnique();

                entity.HasIndex(r => r.PaymentId)
                    .IsUnique();

                entity.Property(r => r.Amount)
                    .HasPrecision(18, 2);

                entity.Property(r => r.PaymentMethod)
                    .HasMaxLength(50);

                entity.Property(r => r.PaymentReference)
                    .HasMaxLength(200);
            });
            modelBuilder.Entity<PaymentRefund>(entity =>
            {
                entity.HasKey(r => r.RefundId);

                entity.Property(r => r.Amount)
                    .HasPrecision(18, 2);

                entity.Property(r => r.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(r => r.Reason)
                    .HasMaxLength(500);

                entity.Property(r => r.IdempotencyKey)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(r => r.IdempotencyKey)
                    .IsUnique();

                entity.HasIndex(r => r.RazorpayRefundId)
                    .IsUnique()
                    .HasFilter("[RazorpayRefundId] IS NOT NULL");

                entity.HasIndex(r => r.PaymentId);
            });

            modelBuilder.Entity<PaymentReconciliation>(entity =>
            {
                entity.HasKey(r => r.ReconciliationId);

                entity.Property(r => r.DatabaseAmount)
                    .HasPrecision(18, 2);

                entity.Property(r => r.GatewayAmount)
                    .HasPrecision(18, 2);

                entity.Property(r => r.PaymentReference)
                    .HasMaxLength(150);

                entity.Property(r => r.DatabaseStatus)
                    .HasMaxLength(50);

                entity.Property(r => r.GatewayStatus)
                    .HasMaxLength(50);

                entity.Property(r => r.Result)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(r => r.Remarks)
                    .HasMaxLength(1000);

                entity.HasIndex(r => new
                {
                    r.PaymentId,
                    r.ReconciliationId
                });
            });

        }
    }
}