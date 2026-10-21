using LoanManagementSystem.API.Middleware;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Application.Service;
using LoanManagementSystem.Application.ServiceInterface;
using LoanManagementSystem.Application.Services;
using LoanManagementSystem.Domain.RepositoryInterfaces;
using LoanManagementSystem.Infrastructure.Clients;
using LoanManagementSystem.Infrastructure.Data;
using LoanManagementSystem.Infrastructure.Notifications;
using LoanManagementSystem.Infrastructure.Payments;
using LoanManagementSystem.Infrastructure.Repository;
using LoanManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
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
        var baseUrl = builder.Configuration["CustomerApi:BaseUrl"]
       ?? throw new InvalidOperationException(
           "CustomerApi:BaseUrl is missing");

        client.BaseAddress = new Uri(baseUrl);
    });

builder.Services.AddHostedService<NotificationWorker>();

builder.Services.AddHttpClient<
    IRazorpayPaymentService,
    RazorpayPaymentService>();



// Other services
builder.Services.AddScoped<JwtService>();

builder.Services.AddHttpClient<IEmiModuleClient, EmiModuleClient>(
    (sp, client) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();

        string url = config["EmiModule:BaseUrl"]
            ?? throw new InvalidOperationException(
                "EmiModule:BaseUrl is missing");

        client.BaseAddress = new Uri(url);
        client.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddScoped<
    IEmiAllocationService,
    EmiAllocationService>();

var app = builder.Build();

// Log a masked Razorpay KeyId at startup for verification (never log secrets).
try
{
    var runtimeKeyId = builder.Configuration["Razorpay:KeyId"]
        ?? Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID");

    if (!string.IsNullOrWhiteSpace(runtimeKeyId))
    {
        string masked;
        if (runtimeKeyId.Length <= 8)
            masked = new string('*', runtimeKeyId.Length);
        else
            masked = runtimeKeyId.Substring(0, 4)
                     + new string('*', Math.Max(0, runtimeKeyId.Length - 8))
                     + runtimeKeyId.Substring(runtimeKeyId.Length - 4);

        app.Logger.LogInformation("Razorpay KeyId in use (masked): {KeyId}", masked);
    }
    else
    {
        app.Logger.LogWarning("Razorpay KeyId not configured.");
    }
}
catch (Exception ex)
{
    // Ensure startup doesn't crash due to logging; configuration validation will handle missing keys later.
    app.Logger.LogWarning(ex, "Failed to read Razorpay KeyId for masked logging.");
}

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

// Log request bodies to help diagnose malformed JSON / model binding problems.
app.UseMiddleware<LoanManagementSystem.API.Middleware.RequestBodyLoggingMiddleware>();

app.UseRouting();

app.UseCors("AngularPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();