using System;
using System.Runtime.Caching;

namespace WebApiCache
{
    public class SynchronizedCacheManager
    {
        private static MemoryCache _cache = MemoryCache.Default;

        static SynchronizedCacheManager()
        {
            Instance = new SynchronizedCacheManager();
        }
        
        public virtual object Get(CacheKey key)
        {
            return _cache.Get(key.FullCacheKey, null);
        }

        public virtual void Remove(CacheKey key)
        {
            _cache.Remove(key.FullCacheKey, null);
        }

        public virtual void Set(CacheKey cacheKey, object value)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromDays(365.0)
            };
            _cache.Set(cacheKey.FullCacheKey, value, policy, null);
        }

        public static SynchronizedCacheManager Instance { get; set; }
    }
}

