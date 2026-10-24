using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using LoanManagementSystem.Domain.Entities;
using System;

namespace LoanManagementSystem.Tests.Integration
{
    internal class TestDbSeeder : IHostedService
    {
        private readonly IServiceProvider _sp;

        public TestDbSeeder(IServiceProvider sp)
        {
            _sp = sp;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Ensure the in-memory database is created and seeded
            db.Database.EnsureCreated();

            if (!db.Customers.Any())
            {
                var cust = new Customer
                {
                    FullName = "Test User",
                    Email = "test@example.com",
                    PhoneNumber = "0000000000",
                    PasswordHash = "x",
                    CreatedAt = DateTime.UtcNow
                };
                db.Customers.Add(cust);
                db.SaveChanges();
            }

            if (!db.KycDocuments.Any())
            {
                var customer = db.Customers.First();
                db.KycDocuments.Add(new KycDocument
                {
                    CustomerId = customer.CustomerId,
                    DocumentType = "ID",
                    VerificationStatus = "Verified",
                    UploadedAt = DateTime.UtcNow
                });
                db.SaveChanges();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
