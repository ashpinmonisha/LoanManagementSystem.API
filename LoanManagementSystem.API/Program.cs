using System.Text;
using System.Security.Claims;
using LoanManagementSystem.Application.Service;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using LoanManagementSystem.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models; // not required

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT Key must contain at least 32 bytes.");
}

// Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),

                RoleClaimType = ClaimTypes.Role
            };
    });

// Authorization
builder.Services.AddAuthorization();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("reservation")));

// Application services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// JWT service
builder.Services.AddScoped<JwtService>();

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
builder.Services.AddScoped<IRiskAssessmentRepository, RiskAssessmentRepository>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IKycDocumentRepository, KycDocumentRepository>();
builder.Services.AddScoped<ICustomerEligibilityRepository, CustomerEligibilityRepository>();
builder.Services.AddScoped<IKycDocumentService, KycDocumentService>();
builder.Services.AddScoped<LoanManagementSystem.Domain.RepositoryInterfaces.IKycDocumentHistoryRepository, LoanManagementSystem.Infrastructure.Repository.KycDocumentHistoryRepository>();

// File storage
builder.Services.AddSingleton<LoanManagementSystem.Application.ServiceInterface.IFileStore, LoanManagementSystem.API.Storage.LocalFileStore>();
builder.Services.AddSingleton<LoanManagementSystem.Application.ServiceInterface.IFileValidator, LoanManagementSystem.API.Services.FileValidator>();

// Health checks
builder.Services.AddHealthChecks();
// Notification configuration
builder.Services.Configure<LoanManagementSystem.Application.Configuration.NotificationSettings>(
    builder.Configuration.GetSection("Notifications"));

// Notification service
builder.Services.AddScoped<LoanManagementSystem.Application.ServiceInterface.INotificationService, LoanManagementSystem.Application.Service.EmailNotificationService>();
builder.Services.AddSingleton<LoanManagementSystem.Application.ServiceInterface.ITemplateRenderer, LoanManagementSystem.API.Services.SimpleTemplateRenderer>();

// KYC configuration
builder.Services.Configure<LoanManagementSystem.Application.Configuration.KycSettings>(
    builder.Configuration.GetSection("Kyc"));

// Hosted services - enable can be toggled via configuration (useful for tests)
var enableBackground = builder.Configuration.GetValue<bool?>("Kyc:EnableBackgroundJobs") ?? true;
if (enableBackground)
{
    builder.Services.AddHostedService<LoanManagementSystem.Application.Service.KycExpiryBackgroundService>();
    builder.Services.AddHostedService<LoanManagementSystem.Application.Service.KycRetentionBackgroundService>();
}

// Swagger - basic setup compatible with standard Swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

// Authentication must run before authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
