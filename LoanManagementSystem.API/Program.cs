using LoanManagementSystem.API.Middleware;
using LoanManagementSystem.Application.Service;
using LoanManagementSystem.Application.Services;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Data;
using LoanManagementSystem.Infrastructure.Repository;
using LoanManagementSystem.Infrastructure.Notifications;
using LoanManagementSystem.Infrastructure.Clients;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Enable console logging for easier debugging
builder.Logging.AddConsole();

// Controllers and common middleware
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// CORS configuration for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ILoanCalculationRepository, LoanCalculationRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
builder.Services.AddScoped<IRiskAssessmentRepository, RiskAssessmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IKycDocumentRepository, KycDocumentRepository>();
builder.Services.AddScoped<ICustomerEligibilityRepository, CustomerEligibilityRepository>();

// Application services
builder.Services.AddScoped<IEmiCalculatorService, EmiCalculatorService>();
builder.Services.AddScoped<ICalculationHistoryService, CalculationHistoryService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IKycDocumentService, KycDocumentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<IPaymentService, PaymentService>();

// 4.3 Outstanding Balance
builder.Services.AddScoped<
    IOutstandingBalanceService,
    OutstandingBalanceService>();

builder.Services.AddHttpClient<
    ILoanModuleClient,
    LoanModuleClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["LoanModuleApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "LoanModuleApi:BaseUrl is missing"));
    });
builder.Services.AddScoped<
    IAdvancedPaymentService,
    AdvancedPaymentService>();

builder.Services.AddScoped<
    IPaymentNotificationSender,
    PaymentNotificationSender>();

builder.Services.AddScoped<
    IPaymentNotificationQueue,
    PaymentNotificationQueue>();

var customerApiUrl =
    builder.Configuration["CustomerApi:BaseUrl"];

if (string.IsNullOrWhiteSpace(customerApiUrl))
{
    throw new InvalidOperationException(
        "Configure CustomerApi:BaseUrl in appsettings.json");
}

builder.Services.AddHttpClient<
    ICustomerContactClient,
    CustomerContactClient>(client =>
    {
        client.BaseAddress = new Uri(customerApiUrl);
    });

builder.Services.AddHostedService<NotificationWorker>();


// Other services
builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// Show detailed exceptions when running in Development to aid debugging
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AngularPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();