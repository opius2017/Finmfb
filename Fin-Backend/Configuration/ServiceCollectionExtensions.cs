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
                    b => b.MigrationsAssembly("FinTech.WebAPI")));

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

            // Notification Services
            services.AddNotificationServices(configuration);

            // Logging
            services.AddLogging();

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

            // JWT Configuration would go here
            // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //     .AddJwtBearer(options => { ... });

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
