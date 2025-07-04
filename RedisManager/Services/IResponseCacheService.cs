using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisManager.Services
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string cacheKey, object responseObject, TimeSpan timeToLive);
        Task<string> GetCachedResponseAsync(string cacheKey);
    }
}
