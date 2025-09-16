using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Fin_Backend.Infrastructure.Caching
{
    public interface IDistributedCacheService
    {
        Task<T> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null) where T : class;
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null) where T : class;
    }

    public class RedisDistributedCacheService : IDistributedCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisDistributedCacheService> _logger;

        public RedisDistributedCacheService(IDistributedCache distributedCache, ILogger<RedisDistributedCacheService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedValue = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached value for key {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null) where T : class
        {
            if (value == null)
            {
                return;
            }

            try
            {
                var options = new DistributedCacheEntryOptions();
                
                if (absoluteExpiration.HasValue)
                {
                    options.SetAbsoluteExpiration(absoluteExpiration.Value);
                }
                
                if (slidingExpiration.HasValue)
                {
                    options.SetSlidingExpiration(slidingExpiration.Value);
                }

                var serializedValue = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, serializedValue, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cached value for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached value for key {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var cachedValue = await _distributedCache.GetAsync(key);
                return cachedValue != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key {Key} exists in cache", key);
                return false;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null) where T : class
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var newValue = await factory();
            if (newValue != null)
            {
                await SetAsync(key, newValue, absoluteExpiration, slidingExpiration);
            }

            return newValue;
        }
    }
}