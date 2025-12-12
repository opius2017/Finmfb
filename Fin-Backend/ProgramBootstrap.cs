using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "FinTech API - Minimal Mode - Backend is starting");
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow, mode = "minimal" });
app.MapGet("/api/status", () => new 
{
    application = "FinTech Cooperative Loan Management System",
    version = "1.0.0",
    status = "online",
    mode = "minimal-bootstrap",
    message = "API is running in minimal mode while fixing compilation errors",
    timestamp = DateTime.UtcNow
});

Console.WriteLine("========================================");
Console.WriteLine("FinTech API - MINIMAL MODE");
Console.WriteLine("http://localhost:5000");
Console.WriteLine("http://localhost:5000/swagger");
Console.WriteLine("========================================");

app.Run("http://localhost:5000");
