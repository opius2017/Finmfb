using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        Description = "Nigerian FinTech Solution API"
    });

    // Add JWT bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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

// Add Identity services for user management
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("FinTechTestDb"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

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
                role = "Admin"
            }
        });
    }
    
    return Results.Unauthorized();
});

// Add a simple endpoint to list Nigerian test users
app.MapGet("/api/users/test-credentials", () =>
{
    var credentials = new[]
    {
        new { Role = "Super Admin", Email = "admin@soarfin.ng", Password = "SoarFin2025!", Name = "Adebayo Ogundimu" },
        new { Role = "MFB Manager", Email = "manager@mfb.soarfin.ng", Password = "MFBManager2025!", Name = "Fatima Abdullahi" },
        new { Role = "SME Owner", Email = "owner@sme.soarfin.ng", Password = "SMEOwner2025!", Name = "Chukwuemeka Igwe" },
        new { Role = "Accountant", Email = "accountant@soarfin.ng", Password = "Accountant2025!", Name = "Aisha Bello" },
        new { Role = "Teller", Email = "teller@soarfin.ng", Password = "Teller2025!", Name = "Olumide Adebayo" }
    };
    
    return Results.Ok(new { TestCredentials = credentials });
});

try
{
    // Seed Nigerian test users in Development environment
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                // Ensure database is created
                context.Database.EnsureCreated();
                
                Log.Information("Seeding Nigerian test users");
                SimpleUserSeeder.SeedNigerianTestUsersAsync(userManager, roleManager).Wait();
                
                // Display credentials in console
                SimpleUserSeeder.DisplayNigerianTestCredentials();
                
                Log.Information("Nigerian test users seeded successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while seeding the database with test users");
            }
        }
    }

    Log.Information("FinTech API application started successfully");
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

// Supporting classes and records
public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string PhoneNumber);
public record UserResponse(string Id, string Email, string Name, string Role);

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}

public static class SimpleUserSeeder
{
    public static async Task SeedNigerianTestUsersAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Create roles if they don't exist
        string[] roles = { "Admin", "User", "Staff", "MFBManager", "Teller" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Nigerian test users data
        var nigerianUsers = new[]
        {
            new { Email = "admin@soarfin.ng", Password = "SoarFin2025!", Role = "Admin", FirstName = "Adebayo", LastName = "Ogundimu", Phone = "+2348012345678" },
            new { Email = "manager@mfb.soarfin.ng", Password = "MFBManager2025!", Role = "MFBManager", FirstName = "Fatima", LastName = "Abdullahi", Phone = "+2348012345679" },
            new { Email = "owner@sme.soarfin.ng", Password = "SMEOwner2025!", Role = "User", FirstName = "Chukwuemeka", LastName = "Igwe", Phone = "+2348012345680" },
            new { Email = "accountant@soarfin.ng", Password = "Accountant2025!", Role = "Staff", FirstName = "Aisha", LastName = "Bello", Phone = "+2348012345681" },
            new { Email = "teller@soarfin.ng", Password = "Teller2025!", Role = "Teller", FirstName = "Olumide", LastName = "Adebayo", Phone = "+2348012345682" }
        };

        foreach (var userData in nigerianUsers)
        {
            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(userData.Email);
            if (existingUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = userData.Email,
                    Email = userData.Email,
                    EmailConfirmed = true,
                    PhoneNumber = userData.Phone,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(user, userData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userData.Role);
                    Console.WriteLine($"Created Nigerian test user: {userData.Email} with role {userData.Role}");
                }
                else
                {
                    Console.WriteLine($"Failed to create user {userData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"User {userData.Email} already exists");
            }
        }
    }

    public static void DisplayNigerianTestCredentials()
    {
        Console.WriteLine("\n=== NIGERIAN TEST USER CREDENTIALS ===\n");
        
        var credentials = new[]
        {
            new { Role = "Super Admin", Email = "admin@soarfin.ng", Password = "SoarFin2025!", Name = "Adebayo Ogundimu" },
            new { Role = "MFB Manager", Email = "manager@mfb.soarfin.ng", Password = "MFBManager2025!", Name = "Fatima Abdullahi" },
            new { Role = "SME Owner", Email = "owner@sme.soarfin.ng", Password = "SMEOwner2025!", Name = "Chukwuemeka Igwe" },
            new { Role = "Accountant", Email = "accountant@soarfin.ng", Password = "Accountant2025!", Name = "Aisha Bello" },
            new { Role = "Teller", Email = "teller@soarfin.ng", Password = "Teller2025!", Name = "Olumide Adebayo" }
        };

        foreach (var cred in credentials)
        {
            Console.WriteLine($"{cred.Role}:");
            Console.WriteLine($"  Name: {cred.Name}");
            Console.WriteLine($"  Email: {cred.Email}");
            Console.WriteLine($"  Password: {cred.Password}");
            Console.WriteLine();
        }
        
        Console.WriteLine("=== END OF CREDENTIALS ===\n");
    }
}