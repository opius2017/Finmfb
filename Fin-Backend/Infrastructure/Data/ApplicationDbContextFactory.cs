using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FinTech.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Get the current directory (should be Infrastructure project folder)
            // But we need to point to WebAPI for appsettings
            // Adjusted for when WebAPI is in the root of Fin-Backend
            var path = Directory.GetCurrentDirectory();
            
            // If running from Infrastructure folder, go up one level
            if (path.EndsWith("Infrastructure"))
            {
                 path = Path.GetFullPath(Path.Combine(path, ".."));
            }
            
            // If appsettings.json doesn't exist here, try hardcoded path
            if (!File.Exists(Path.Combine(path, "appsettings.json")))
            {
                path = @"C:\Users\opius\Desktop\projectFin\Finmfb\Fin-Backend";
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("FinTech.Infrastructure"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
