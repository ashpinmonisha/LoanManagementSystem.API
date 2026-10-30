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

using QuestPDF.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 1. CONTROLLERS - POSTMAN REST API
// =====================================================

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Allows internal module-to-module HTTP calls to reuse the caller's JWT.
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ForwardAuthorizationHandler>();

builder.Logging.AddConsole();

// =====================================================
// 2. JWT AUTHENTICATION
// =====================================================

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
                    Encoding.UTF8.GetBytes(jwtKey)),

                ClockSkew = TimeSpan.FromMinutes(1)
            };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext
                    .RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuthentication");

                logger.LogWarning(
                    context.Exception,
                    "JWT authentication failed");

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// =====================================================
// 3. SQL SERVER DATABASE
// =====================================================

var connectionString = builder.Configuration
    .GetConnectionString("reservation");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:reservation is missing.");
}

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        options.UseSqlServer(connectionString);
    });

// =====================================================
// 4. ANGULAR CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =====================================================
// 5. REPOSITORIES
// =====================================================

// Loan Calculation
builder.Services.AddScoped<
    ILoanCalculationRepository,
    LoanCalculationRepository>();

// Employee
builder.Services.AddScoped<
    IEmployeeRepository,
    EmployeeRepository>();

// Loan
builder.Services.AddScoped<
    ILoanApplicationRepository,
    LoanApplicationRepository>();

// Risk Assessment
builder.Services.AddScoped<
    IRiskAssessmentRepository,
    RiskAssessmentRepository>();

// Customer
builder.Services.AddScoped<
    ICustomerRepository,
    CustomerRepository>();

// KYC
builder.Services.AddScoped<
    IKycDocumentRepository,
    KycDocumentRepository>();

// Customer Eligibility
builder.Services.AddScoped<
    ICustomerEligibilityRepository,
    CustomerEligibilityRepository>();

// Payment
builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();

// MODULE 4.3 - LOAN BALANCE DATA READER
builder.Services.AddScoped<
    ILoanOutstandingDataReader,
    LoanOutstandingDataReader>();

// =====================================================
// 6. APPLICATION SERVICES
// =====================================================

// Customer Eligibility
builder.Services.AddScoped<
    ICustomerEligibilityService,
    CustomerEligibilityService>();

// EMI Calculation
builder.Services.AddScoped<
    IEmiCalculatorService,
    EmiCalculatorService>();

// Calculation History
builder.Services.AddScoped<
    ICalculationHistoryService,
    CalculationHistoryService>();

// Employee
builder.Services.AddScoped<
    IEmployeeService,
    EmployeeService>();

// Loan
builder.Services.AddScoped<
    ILoanService,
    LoanService>();

// Risk
builder.Services.AddScoped<
    IRiskAssessmentService,
    RiskAssessmentService>();

// Customer
builder.Services.AddScoped<
    ICustomerService,
    CustomerService>();

// KYC
builder.Services.AddScoped<
    IKycDocumentService,
    KycDocumentService>();

// MODULE 4.1 - RECORD PAYMENT
builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

// MODULE 4.3 - OUTSTANDING BALANCE
builder.Services.AddScoped<
    IOutstandingBalanceService,
    OutstandingBalanceService>();

// MODULE 4.4 - ADVANCED PAYMENT
builder.Services.AddScoped<
    IAdvancedPaymentService,
    AdvancedPaymentService>();

// EMI Allocation
builder.Services.AddScoped<
    IEmiAllocationService,
    EmiAllocationService>();

// JWT Token Generator
builder.Services.AddScoped<JwtService>();

// =====================================================
// 7. LOAN MODULE HTTP CLIENT
// =====================================================

builder.Services.AddHttpClient<
    ILoanModuleClient,
    LoanModuleClient>(client =>
    {
        client.BaseAddress = GetRequiredBaseUri(
            builder.Configuration,
            "LoanModuleApi:BaseUrl");

        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<ForwardAuthorizationHandler>();

// =====================================================
// 8. CUSTOMER CONTACT HTTP CLIENT
// =====================================================

builder.Services.AddHttpClient<
    ICustomerContactClient,
    CustomerContactClient>(client =>
    {
        client.BaseAddress = GetRequiredBaseUri(
            builder.Configuration,
            "CustomerApi:BaseUrl");

        client.Timeout = TimeSpan.FromSeconds(30);
    });

// =====================================================
// 9. EMI MODULE HTTP CLIENT
// =====================================================
// 9. EMI MODULE HTTP CLIENT

builder.Services.AddHttpClient<
    IEmiModuleClient,
    EmiModuleClient>(client =>
    {
        client.BaseAddress = GetRequiredBaseUri(
            builder.Configuration,
            "EmiModule:BaseUrl");

        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<ForwardAuthorizationHandler>();

// =====================================================
// 10. PAYMENT NOTIFICATIONS
// =====================================================

builder.Services.AddScoped<
    IPaymentNotificationSender,
    PaymentNotificationSender>();

builder.Services.AddScoped<
    IPaymentNotificationQueue,
    PaymentNotificationQueue>();

builder.Services.AddHostedService<NotificationWorker>();

// =====================================================
// 11. RAZORPAY PAYMENTS
// =====================================================

builder.Services.AddHttpClient<
    IRazorpayPaymentService,
    RazorpayPaymentService>();

// =====================================================
// 12. MODULE 4.7 - PAYMENT RECEIPT
// =====================================================

builder.Services.AddScoped<
    IPaymentReceiptRepository,
    PaymentReceiptRepository>();

builder.Services.AddScoped<
    IPaymentReceiptService,
    PaymentReceiptService>();

builder.Services.AddScoped<
    IPaymentReceiptPdfGenerator,
    PaymentReceiptPdfGenerator>();

// =====================================================
// 13. MODULE 4.8 - PAYMENT REFUND
// =====================================================

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

// =====================================================
// 14. MODULE 4.9 - PAYMENT RECONCILIATION
// =====================================================

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

// =====================================================
// 15. MODULE 4.10 - PAYMENT REPORTS
// =====================================================

builder.Services.AddScoped<
    IPaymentReportRepository,
    PaymentReportRepository>();

builder.Services.AddScoped<
    IPaymentReportService,
    PaymentReportService>();

builder.Services.AddScoped<
    IPaymentReportExporter,
    PaymentReportExporter>();

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// =====================================================
// 16. BUILD APPLICATION AND SHOW DI ERRORS
// =====================================================

WebApplication app;

try
{
    app = builder.Build();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine(
        "========== APPLICATION STARTUP FAILED ==========");

    Console.WriteLine(ex.ToString());

    if (ex is AggregateException aggregateException)
    {
        foreach (var inner in
            aggregateException.Flatten().InnerExceptions)
        {
            Console.WriteLine(
                "DEPENDENCY ERROR: " + inner.Message);
        }
    }

    Console.ResetColor();

    throw;
}

// =====================================================
// 17. EXCEPTION HANDLING
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
    app.UseHttpsRedirection();
}

// =====================================================
// 18. MIDDLEWARE
// =====================================================

app.UseRouting();

app.UseCors("AngularPolicy");

app.UseAuthentication();

app.UseAuthorization();

// =====================================================
// 19. MAP API CONTROLLERS
// =====================================================

app.MapControllers();

// =====================================================
// 20. START THE APPLICATION
// =====================================================

app.Run();

// =====================================================
// HELPER - VALIDATE EXTERNAL API URLs
// =====================================================

static Uri GetRequiredBaseUri(
    IConfiguration configuration,
    string configurationKey)
{
    var url = configuration[configurationKey];

    if (string.IsNullOrWhiteSpace(url) ||
        !Uri.TryCreate(
            url,
            UriKind.Absolute,
            out var baseUri) ||
        (baseUri.Scheme != Uri.UriSchemeHttp &&
         baseUri.Scheme != Uri.UriSchemeHttps))
    {
        throw new InvalidOperationException(
            $"Configure a valid HTTP/HTTPS URL for '{configurationKey}'.");
    }

    return baseUri;
}