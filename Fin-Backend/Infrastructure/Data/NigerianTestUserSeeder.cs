using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Infrastructure.Data;
using FinTech.Domain.Entities.Customers;
using FinTech.Domain.Enums;

namespace FinTech.Infrastructure.Data
{
    public static class NigerianTestUserSeeder
    {
        public static async Task SeedNigerianTestUsersAsync(ApplicationDbContext context)
        {
            var hasher = new PasswordHasher<IdentityUser>();
            
            // Create roles if they don't exist
            if (!await context.Roles.AnyAsync(r => r.Name == "Admin"))
            {
                context.Roles.Add(new IdentityRole 
                { 
                    Id = Guid.NewGuid().ToString(),
                    Name = "Admin", 
                    NormalizedName = "ADMIN", 
                    ConcurrencyStamp = Guid.NewGuid().ToString() 
                });
            }
            
            if (!await context.Roles.AnyAsync(r => r.Name == "Staff"))
            {
                context.Roles.Add(new IdentityRole 
                { 
                    Id = Guid.NewGuid().ToString(),
                    Name = "Staff", 
                    NormalizedName = "STAFF", 
                    ConcurrencyStamp = Guid.NewGuid().ToString() 
                });
            }
            
            if (!await context.Roles.AnyAsync(r => r.Name == "User"))
            {
                context.Roles.Add(new IdentityRole 
                { 
                    Id = Guid.NewGuid().ToString(),
                    Name = "User", 
                    NormalizedName = "USER", 
                    ConcurrencyStamp = Guid.NewGuid().ToString() 
                });
            }
            
            await context.SaveChangesAsync();
            
            // Create Admin User
            var adminUser = await CreateUserIfNotExists(
                context,
                hasher,
                "admin@finmfb.com.ng",
                "Admin@123",
                "Admin",
                "Oluwaseun", 
                "Adebayo", 
                "Lagos",
                "08012345678",
                "12 Marina Street, Lagos Island"
            );
            
            // Create Staff User
            var staffUser = await CreateUserIfNotExists(
                context,
                hasher,
                "staff@finmfb.com.ng",
                "Staff@123",
                "Staff",
                "Chioma", 
                "Okonkwo", 
                "Abuja",
                "08023456789",
                "45 Wuse Zone 2, Abuja"
            );
            
            // Create Regular User
            var regularUser = await CreateUserIfNotExists(
                context,
                hasher,
                "user@finmfb.com.ng",
                "User@123",
                "User",
                "Emeka", 
                "Nwachukwu", 
                "Port Harcourt",
                "08034567890",
                "78 Aba Road, Port Harcourt"
            );
            
            await context.SaveChangesAsync();
        }
        
        private static async Task<IdentityUser> CreateUserIfNotExists(
            ApplicationDbContext context,
            PasswordHasher<IdentityUser> hasher,
            string email,
            string password,
            string role,
            string firstName,
            string lastName,
            string city,
            string phoneNumber,
            string address)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
                // Create Identity User
                user = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    NormalizedUserName = email.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true,
                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false
                };
                
                user.PasswordHash = hasher.HashPassword(user, password);
                context.Users.Add(user);
                await context.SaveChangesAsync();
                
                // Assign role
                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == role);
                if (userRole != null)
                {
                    context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        UserId = user.Id,
                        RoleId = userRole.Id
                    });
                    await context.SaveChangesAsync();
                }
                
                // Create Customer Profile
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    CustomerNumber = "NG" + new Random().Next(1000000, 9999999).ToString(),
                    CustomerType = CustomerType.Individual,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Address = address,
                    City = city,
                    State = "Nigeria",
                    Country = "Nigeria",
                    Status = CustomerStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Gender = Gender.Other,
                    DateOfBirth = new DateTime(1985, 1, 1),
                    BVN = "222" + new Random().Next(10000000, 99999999).ToString(),
                    NIN = "12345" + new Random().Next(10000, 99999).ToString(),
                    AccountOfficerId = null,
                    UserId = user.Id
                };
                
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
                
                // Create security preferences
                context.SecurityPreferences.Add(new Fin_Backend.Domain.Entities.Authentication.UserSecurityPreferences
                {
                    UserId = user.Id,
                    NotifyOnLogin = true,
                    NotifyOnPasswordChange = true,
                    NotifyOnProfileUpdate = true,
                    EnableLoginAlerts = true,
                    EnableSuspiciousActivityAlerts = true,
                    UseStrictDeviceVerification = false,
                    LastUpdated = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                
                // Create MFA settings (disabled by default)
                context.UserMfaSettings.Add(new Fin_Backend.Domain.Entities.Authentication.MfaSettings
                {
                    UserId = user.Id,
                    IsEnabled = false,
                    Method = "App",
                    SharedKey = "TESTSECRETKEY" + new Random().Next(1000, 9999).ToString()
                });
                await context.SaveChangesAsync();
            }
            
            return user;
        }
    }
}