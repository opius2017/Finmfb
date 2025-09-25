using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace FinTech.Infrastructure.Data.Configuration;

public static class ModelBuilderExtensions
{
    public static void ApplyAllConfigurations(this ModelBuilder modelBuilder)
    {
        var applyConfigurationMethodInfo = modelBuilder
            .GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(m => m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));

        var assembly = Assembly.GetExecutingAssembly();
        
        var configurationTypes = assembly
            .GetTypes()
            .Where(type => 
                !type.IsAbstract && 
                !type.IsGenericTypeDefinition && 
                type.GetInterfaces()
                    .Any(i => i.IsGenericType && 
                              i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)));

        foreach (var configurationType in configurationTypes)
        {
            var entityType = configurationType
                .GetInterfaces()
                .First(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                .GetGenericArguments()[0];

            var applyConfigMethodInfo = applyConfigurationMethodInfo.MakeGenericMethod(entityType);
            
            var configuration = Activator.CreateInstance(configurationType);
            
            applyConfigMethodInfo.Invoke(modelBuilder, new[] { configuration });
        }
    }
}
