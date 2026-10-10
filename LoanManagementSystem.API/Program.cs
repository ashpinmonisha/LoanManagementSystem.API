
using LoanManagementSystem.Application.Service;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using LoanManagementSystem.Infrastructure.Repository;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddScoped<IKycDocumentService, KycDocumentService>();


builder.Services.AddScoped<
    ICustomerEligibilityService,
    CustomerEligibilityService>();

// Swagger + JWT Bearer Authentication
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

   
options.AddSecurityRequirement(document =>
{
    var requirement = new OpenApiSecurityRequirement();
    requirement.Add(
        new OpenApiSecuritySchemeReference("Bearer", document),
        new List<string>());

    return requirement;
});

});

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT Key is missing from appsettings.json");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer is missing from appsettings.json");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience is missing from appsettings.json");

// Authentication
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

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
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
builder.Services.AddScoped<ICustomerService, CustomerService>();

// JWT Service
builder.Services.AddScoped<JwtService>();

// Existing Repositories
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

// Controllers
app.MapControllers();

app.Run();
