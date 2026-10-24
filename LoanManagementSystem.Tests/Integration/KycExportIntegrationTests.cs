using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Infrastructure.Data;
using LoanManagementSystem.Domain.Entities;
using System;
using System.Linq;
using Xunit;

namespace LoanManagementSystem.Tests.Integration;

public class KycExportIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public KycExportIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, Microsoft.Extensions.Logging.ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, Microsoft.AspNetCore.Authentication.ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "testuser"), new Claim(ClaimTypes.Role, "Admin") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    [Fact]
    public async Task ExportCsv_ReturnsStreamedCsv()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    // disable background jobs in test environment
                    var dict = new Dictionary<string, string?> { ["Kyc:EnableBackgroundJobs"] = "false" };
                    cfg.AddInMemoryCollection(dict);
                });

                builder.ConfigureTestServices(services =>
            {
                // replace DB with in-memory
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.RemoveAll(typeof(ApplicationDbContext));
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));

                // remove hosted services to avoid background jobs running during tests
                services.RemoveAll(typeof(Microsoft.Extensions.Hosting.IHostedService));
                // Allow our TestDbSeeder to run - keep hosted services removed first then re-register seeder

                // add test authentication and make it the default scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // do not register additional hosted services here; we'll seed the in-memory DB after host starts
            });
        });

        // seed the in-memory database via the factory's service provider
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
        }

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var resp = await client.GetAsync("/api/KycDocument/export");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("text/csv", resp.Content.Headers.ContentType?.MediaType);
        var content = await resp.Content.ReadAsStringAsync();
        Assert.Contains("KycDocumentId", content);
    }
}
