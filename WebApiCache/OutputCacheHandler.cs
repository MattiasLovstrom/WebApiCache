using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;

namespace WebApiCache
{
    public class OutputCacheHandler
    {
        public static readonly TimeSpan MaximunUpdateTime = TimeSpan.FromMinutes(10.0);

        public static HttpResponseMessageWrapper Get(CacheKey cacheKey)
        {
            HttpResponseMessageWrapper response = SynchronizedCacheManager.Instance.Get(cacheKey) as HttpResponseMessageWrapper;
            if (response == null)
            {
                return null;
            }
            if (response.Invalidated != null
                && response.Invalidated.Value.Add(MaximunUpdateTime) < DateTime.UtcNow)
            {
                SynchronizedCacheManager.Instance.Remove(cacheKey);
                return null;
            }
            return response;
        }

        public static void InvalidateOutputCache(CacheKey cacheKey)
        {
            HttpResponseMessageWrapper response = SynchronizedCacheManager.Instance.Get(cacheKey) as HttpResponseMessageWrapper;
            if (response != null)
            {
                response.Invalidated = new DateTime?(DateTime.UtcNow);
            }
        }

        

        public static void Invalidate(CacheKey cacheKey)
        {
            InvalidateOutputCache(cacheKey);
            ETagStore.Invalidate(cacheKey);
        }
    }
}

