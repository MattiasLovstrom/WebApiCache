using System;
using System.Net.Http;
using System.Runtime.Caching;

namespace WebApiCache
{
    public class OutputCacheHandler
    {
        private static MemoryCache _cache = MemoryCache.Default;
        public static readonly TimeSpan MaximunUpdateTime = TimeSpan.FromMinutes(10.0);

        public static HttpResponseMessageWrapper Get(string cacheKey)
        {
            HttpResponseMessageWrapper response = _cache.Get(cacheKey, null) as HttpResponseMessageWrapper;
            if (response == null)
            {
                return null;
            }
            if (response.Invalidated != null
                && response.Invalidated.Value.Add(MaximunUpdateTime) < DateTime.UtcNow)
            {
                Remove(cacheKey);
                return null;
            }
            return response;
        }

        public static void Invalidate(string cacheKey)
        {
            HttpResponseMessageWrapper response = _cache.Get(cacheKey, null) as HttpResponseMessageWrapper;
            if (response != null)
            {
                response.Invalidated = new DateTime?(DateTime.UtcNow);
            }
        }

        public static void Remove(string cacheKey)
        {
            _cache.Remove(cacheKey, null);
        }

        public static void Set(string cacheKey, HttpResponseMessageWrapper response)
        {
            CacheItemPolicy policy = new CacheItemPolicy {
                SlidingExpiration = TimeSpan.FromDays(1.0)
            };
            _cache.Set(cacheKey, response, policy, null);
        }
    }
}

