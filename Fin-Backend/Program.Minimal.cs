using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Minimal services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "FinTech API", 
        Version = "v1",
        Description = "Cooperative Loan Management System API - Minimal Mode"
    });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinTech API V1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.MapGet("/", () => Results.Redirect("/swagger"));

Console.WriteLine("========================================");
Console.WriteLine("FinTech API Starting (Minimal Mode)");
Console.WriteLine("Swagger UI: http://localhost:5000/swagger");
Console.WriteLine("Health: http://localhost:5000/health");
Console.WriteLine("========================================");

app.Run();
