
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
using LoanManagementSystem.Infrastructure.Repositories;
using LoanManagementSystem.Infrastructure.Repository;
using LoanManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
<<<<<<< HEAD
using QuestPDF.Infrastructure;
using System.Net.Http.Headers;
=======
>>>>>>> origin/feature_dev1
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must contain at least 32 bytes.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Jwt:Issuer and Jwt:Audience must be configured.");
}

// Controllers, Swagger and error handling
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Angular CORS
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

// JWT authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
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

builder.Services.AddAuthorization();

// Database
var connectionString =
    builder.Configuration.GetConnectionString("reservation");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:reservation is missing.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories
builder.Services.AddScoped<ILoanCalculationRepository, LoanCalculationRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
builder.Services.AddScoped<IRiskAssessmentRepository, RiskAssessmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IKycDocumentRepository, KycDocumentRepository>();
builder.Services.AddScoped<ICustomerEligibilityRepository, CustomerEligibilityRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Application services
builder.Services.AddScoped<ICustomerEligibilityService, CustomerEligibilityService>();
builder.Services.AddScoped<IEmiCalculatorService, EmiCalculatorService>();
builder.Services.AddScoped<ICalculationHistoryService, CalculationHistoryService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IKycDocumentService, KycDocumentService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOutstandingBalanceService, OutstandingBalanceService>();
builder.Services.AddScoped<IAdvancedPaymentService, AdvancedPaymentService>();
builder.Services.AddScoped<IEmiAllocationService, EmiAllocationService>();
builder.Services.AddScoped<JwtService>();

// Loan module HTTP client
builder.Services.AddHttpClient<ILoanModuleClient, LoanModuleClient>(
    client =>
    {
        client.BaseAddress = GetRequiredBaseUri(
            builder.Configuration, "LoanModuleApi:BaseUrl");
    });

// Customer contact HTTP client
builder.Services.AddHttpClient<ICustomerContactClient, CustomerContactClient>(
    client =>
    {
        client.BaseAddress = GetRequiredBaseUri(
            builder.Configuration, "CustomerApi:BaseUrl");
    });

// EMI module HTTP client
builder.Services.AddHttpClient<IEmiModuleClient, EmiModuleClient>(
    client =>
    {
        client.BaseAddress = GetRequiredBaseUri(
            builder.Configuration, "EmiModule:BaseUrl");

        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Payment notifications
builder.Services.AddScoped<IPaymentNotificationSender, PaymentNotificationSender>();
builder.Services.AddScoped<IPaymentNotificationQueue, PaymentNotificationQueue>();

// Background worker
builder.Services.AddHostedService<NotificationWorker>();

// Razorpay
builder.Services.AddHttpClient<IRazorpayPaymentService, RazorpayPaymentService>();

// Module 4.7 - Payment Receipt Generation

builder.Services.AddScoped<
    IPaymentReceiptRepository,
    PaymentReceiptRepository>();

builder.Services.AddScoped<
    IPaymentReceiptService,
    PaymentReceiptService>();

builder.Services.AddScoped<
    IPaymentReceiptPdfGenerator,
    PaymentReceiptPdfGenerator>();

QuestPDF.Settings.License =
    QuestPDF.Infrastructure.LicenseType.Community;

// Module 4.8 - Payment Refund Management

builder.Services.AddScoped<
    IPaymentRefundRepository,
    PaymentRefundRepository>();

builder.Services.AddScoped<
    IPaymentRefundService,
    PaymentRefundService>();

builder.Services.AddHttpClient<
    IRefundGatewayClient,
    RazorpayRefundClient>(client =>
    {
        client.BaseAddress =
            new Uri("https://api.razorpay.com/v1/");

        client.Timeout = TimeSpan.FromSeconds(30);
    });


// Module 4.9 - Payment Reconciliation

builder.Services.AddScoped<
    IPaymentReconciliationRepository,
    PaymentReconciliationRepository>();

builder.Services.AddScoped<
    IPaymentReconciliationService,
    PaymentReconciliationService>();

builder.Services.AddHttpClient<
    IPaymentGatewayVerificationClient,
    RazorpayVerificationClient>(client =>
    {
        client.BaseAddress =
            new Uri("https://api.razorpay.com/v1/");

        client.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddScoped<
    IPaymentReportRepository,
    PaymentReportRepository>();

builder.Services.AddScoped<
    IPaymentReportService,
    PaymentReportService>();

builder.Services.AddScoped<
    IPaymentReportExporter,
    PaymentReportExporter>();

// Use Community only if your organization is eligible
// under QuestPDF's licensing terms.
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// Log only a masked Razorpay KeyId
var razorpayKeyId =
    builder.Configuration["Razorpay:KeyId"]
    ?? Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID");

if (!string.IsNullOrWhiteSpace(razorpayKeyId))
{
    var maskedKey = razorpayKeyId.Length <= 8
        ? new string('*', razorpayKeyId.Length)
        : razorpayKeyId[..4]
          + new string('*', razorpayKeyId.Length - 8)
          + razorpayKeyId[^4..];

    app.Logger.LogInformation(
        "Razorpay KeyId configured (masked): {KeyId}", maskedKey);
}
else
{
    app.Logger.LogWarning("Razorpay KeyId is not configured.");
}

// Exception handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AngularPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Validate HTTP client URLs
static Uri GetRequiredBaseUri(
    IConfiguration configuration,
    string configurationKey)
{
    var url = configuration[configurationKey];

    if (string.IsNullOrWhiteSpace(url) ||
        !Uri.TryCreate(url, UriKind.Absolute, out var baseUri) ||
        (baseUri.Scheme != Uri.UriSchemeHttp &&
         baseUri.Scheme != Uri.UriSchemeHttps))
    {
        throw new InvalidOperationException(
            $"Configure a valid HTTP/HTTPS URL for '{configurationKey}' in appsettings.json.");
    }

    return baseUri;
}
