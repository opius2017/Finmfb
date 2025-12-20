using FinTech.Core.Application.Services.Loans;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Domain.Entities.Identity;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using FinTech.Infrastructure.Data;
using FinTech.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Infrastructure;
using Microsoft.AspNetCore.Identity;
using FinTech.Core.Application.Interfaces;
using FinTech.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinTech.Core.Application;
using FinTech.Infrastructure.Services;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Infrastructure.Repositories.Accounting;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Infrastructure.Repositories.Loans;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Interfaces.Reports;

namespace FinTech.Configuration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all application services for dependency injection
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("FinTech.Infrastructure")));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Generic Repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Loan Management Services
            services.AddScoped<ILoanCalculatorService, LoanCalculatorService>();
            services.AddScoped<ILoanEligibilityService, LoanEligibilityService>();
            services.AddScoped<IGuarantorService, GuarantorService>();
            services.AddScoped<ILoanCommitteeService, LoanCommitteeService>();
            services.AddScoped<ILoanRegisterService, LoanRegisterService>();
            services.AddScoped<IMonthlyThresholdService, MonthlyThresholdService>();
            services.AddScoped<ILoanDisbursementService, LoanDisbursementService>();
            services.AddScoped<ILoanRepaymentService, LoanRepaymentService>();
            services.AddScoped<ILoanRepaymentService, LoanRepaymentService>();
            services.AddScoped<IDelinquencyManagementService, DelinquencyManagementService>();
            services.AddScoped<IMfaService, MfaService>();

            // Notification Services
            services.AddNotificationServices(configuration);

            // Dashboard Service
            services.AddScoped<FinTech.Core.Application.Interfaces.IDashboardService, FinTech.Core.Application.Services.Dashboard.DashboardService>();

            // Report Service
            services.AddScoped<IReportService, FinTech.Core.Application.Services.Reports.ReportService>();

            // Logging
            services.AddLogging();
            
            // HTTP Client
            services.AddHttpClient();
            
            // Register Repositories
            services.AddScoped<IChartOfAccountRepository, ChartOfAccountRepository>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<IFinancialPeriodRepository, FinancialPeriodRepository>();
            services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
            services.AddScoped<ILoanTransactionRepository, LoanTransactionRepository>();
            services.AddScoped<IFiscalYearRepository, FiscalYearRepository>();
            services.AddScoped<ILoanRepaymentScheduleRepository, LoanRepaymentScheduleRepository>();

            // Core Application Services (MediatR, AutoMapper, etc.)
            FinTech.Core.Application.DependencyInjection.AddApplicationServices(services);

            // Current User Service
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }

        /// <summary>
        /// Registers authentication and authorization services
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Identity Configuration
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options => 
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT Configuration
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Configure Application Cookie to prevent redirection issues when mixing Identity and JWT
            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization(options =>
            {
                // Define policies
                options.AddPolicy("MemberOnly", policy => policy.RequireRole("Member"));
                options.AddPolicy("CommitteeOnly", policy => policy.RequireRole("Committee", "Admin"));
                options.AddPolicy("FinanceOnly", policy => policy.RequireRole("Finance", "Admin"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
            });

            return services;
        }

        /// <summary>
        /// Registers CORS policies
        /// </summary>
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });

                options.AddPolicy("Production", builder =>
                {
                    builder.WithOrigins("https://yourdomain.com", "https://app.yourdomain.com")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// Registers caching services
        /// </summary>
        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Memory Cache
            services.AddMemoryCache();

            // Distributed Cache (Redis) - Optional
            // services.AddStackExchangeRedisCache(options =>
            // {
            //     options.Configuration = configuration.GetConnectionString("Redis");
            //     options.InstanceName = "CooperativeLoan_";
            // });

            return services;
        }

        /// <summary>
        /// Registers background job services (Hangfire)
        /// </summary>
        public static IServiceCollection AddBackgroundJobServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Hangfire Configuration
            // services.AddHangfire(config =>
            // {
            //     config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
            // });
            // services.AddHangfireServer();

            return services;
        }
    }
}
