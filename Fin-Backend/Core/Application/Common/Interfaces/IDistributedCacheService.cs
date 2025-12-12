using System;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Common.Interfaces
{
    /// <summary>
    /// Interface for distributed caching service
    /// </summary>
    public interface IDistributedCacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    }
}
