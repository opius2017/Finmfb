using FinTech.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/cooperative-loan-system-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use PascalCase
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Application Services
builder.Services.AddApplicationServices(builder.Configuration);

// CORS
builder.Services.AddCorsConfiguration();

// Authentication & Authorization
builder.Services.AddAuthenticationConfiguration(builder.Configuration);

// Caching
builder.Services.AddCachingConfiguration(builder.Configuration);

// Background Jobs
builder.Services.AddBackgroundJobConfiguration(builder.Configuration);

// Health Checks
builder.Services.AddHealthCheckConfiguration(builder.Configuration);

// Swagger Documentation
builder.Services.AddSwaggerDocumentation();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Swagger (available in all environments for now)
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

// CORS
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");

// Serilog request logging
app.UseSerilogRequestLogging();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

// Welcome endpoint
app.MapGet("/", () => new
{
    service = "Cooperative Loan Management System API",
    version = "v1.0",
    status = "Running",
    documentation = "/api-docs",
    health = "/health"
});

try
{
    Log.Information("Starting Cooperative Loan Management System API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
