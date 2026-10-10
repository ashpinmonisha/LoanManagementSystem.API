using LoanManagementSystem.Application.Service;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using LoanManagementSystem.Infrastructure.Repository;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException(
                        "JWT Key is missing from appsettings.json")))
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("reservation")));

// Application Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();

// Customer Service
builder.Services.AddScoped<ICustomerService, CustomerService>();

// JWT Service
builder.Services.AddScoped<JwtService>();

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
builder.Services.AddScoped<IRiskAssessmentRepository, RiskAssessmentRepository>();

// Customer & KYC Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IKycDocumentRepository, KycDocumentRepository>();
builder.Services.AddScoped<ICustomerEligibilityRepository, CustomerEligibilityRepository>();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map API Controllers
app.MapControllers();

app.Run();