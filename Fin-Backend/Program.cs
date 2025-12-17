using FinTech.Configuration;
using FinTech.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/cooperative-loan-.txt", rollingInterval: RollingInterval.Day)
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

// Authentication & Authorization
builder.Services.AddAuthenticationServices(builder.Configuration);

// CORS
builder.Services.AddCorsConfiguration();

// Caching
builder.Services.AddCachingServices(builder.Configuration);

// Background Jobs
builder.Services.AddBackgroundJobServices(builder.Configuration);

// Swagger Documentation
// builder.Services.AddSwaggerDocumentation(); // Commented out to reduce dependency issues if missing

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

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

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// CORS
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");

// Serilog Request Logging
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
    documentation = "/swagger/index.html",
    health = "/health"
});

try
{
    Log.Information("Starting Cooperative Loan Management System API");
    Console.WriteLine("DEBUG: Calling app.Run()");
    app.Run();
    Console.WriteLine("DEBUG: app.Run() returned");
}
catch (Exception ex)
{
    Console.WriteLine($"DEBUG: Exception caught: {ex.Message}");
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Console.WriteLine("DEBUG: Finally block executed");
    Log.CloseAndFlush();
}

