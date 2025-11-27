using FinTech.Infrastructure.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FinTech.Infrastructure
{
    public static class CachingServicesRegistration
    {
        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Redis distributed cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis") 
                    ?? configuration.GetValue<string>("Redis:ConnectionString");
                options.InstanceName = "FinMFB:";
            });

            // Register the Redis distributed cache service
            services.AddSingleton<IDistributedCacheService, RedisDistributedCacheService>();

            return services;
        }
    }
}
