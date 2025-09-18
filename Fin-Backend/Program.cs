using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/fintech-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FinTech API",
        Version = "v1",
        Description = "Enterprise Financial Management System API"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinTech API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Add a simple API endpoint to test
app.MapGet("/api/status", () => new
{
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
});

// Add basic auth endpoints for demo
app.MapPost("/api/auth/login", (LoginRequest request) =>
{
    if (request.Email == "admin@demo.com" && request.Password == "Password123!")
    {
        return Results.Ok(new
        {
            token = "demo-jwt-token-" + Guid.NewGuid().ToString("N")[..16],
            user = new
            {
                id = "1",
                email = request.Email,
                name = "Demo Admin",
                role = "Administrator"
            },
            expires = DateTime.UtcNow.AddHours(24)
        });
    }
    
    return Results.Unauthorized();
});

app.MapGet("/api/dashboard", () =>
{
    return Results.Ok(new
    {
        totalCustomers = 1250,
        totalLoans = 2450000.00m,
        pendingApprovals = 15,
        monthlyGrowth = 12.5m,
        recentTransactions = new[]
        {
            new { id = 1, description = "Loan Disbursement", amount = 50000.00m, date = DateTime.UtcNow.AddHours(-2) },
            new { id = 2, description = "Deposit", amount = 25000.00m, date = DateTime.UtcNow.AddHours(-4) },
            new { id = 3, description = "Withdrawal", amount = -15000.00m, date = DateTime.UtcNow.AddHours(-6) }
        }
    });
});

try
{
    Log.Information("Starting FinTech Web API");
    
    // Seed test users in Development environment
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<FinTech.Infrastructure.Data.ApplicationDbContext>();
            
            try
            {
                Log.Information("Seeding Nigerian test users");
                await FinTech.Infrastructure.Data.NigerianTestUserSeeder.SeedNigerianTestUsersAsync(context);
                Log.Information("Nigerian test users seeded successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while seeding the database with test users");
            }
        }
    }
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public record LoginRequest(string Email, string Password);