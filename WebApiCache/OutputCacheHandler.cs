using System;
using System.Net.Http;
using System.Runtime.Caching;

namespace WebApiCache
{
    public class OutputCacheHandler
    {
        private static MemoryCache _cache = MemoryCache.Default;
        public static readonly TimeSpan MaximunUpdateTime = TimeSpan.FromMinutes(10.0);

        public static HttpResponseMessage Get(Type declaringType)
        {
            CachedResponse response = _cache.Get(GetCacheKey(declaringType), null) as CachedResponse;
            if (response == null)
            {
                return null;
            }
            if (response.Invalidated != null
                && DateTime.UtcNow.Add(MaximunUpdateTime) > response.Invalidated.Value)
            {
                Remove(declaringType);
                return null;
            }
            return response.Response;
        }

        public static string GetCacheKey(Type type)
        {
            return ("WebAPiCache" + type.FullName);
        }

        public static void Invalidate(Type declaringType)
        {
            CachedResponse response = _cache.Get(GetCacheKey(declaringType), null) as CachedResponse;
            if (response != null)
            {
                response.Invalidated = new DateTime?(DateTime.UtcNow);
            }
        }

        public static void Remove(Type declaringType)
        {
            _cache.Remove(GetCacheKey(declaringType), null);
        }

        public static void Set(Type declaringType, HttpResponseMessage response)
        {
            CacheItemPolicy policy = new CacheItemPolicy {
                SlidingExpiration = TimeSpan.FromDays(1.0)
            };
            _cache.Set(GetCacheKey(declaringType), new CachedResponse(response), policy, null);
        }
    }
}

