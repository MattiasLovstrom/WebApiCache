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
        private static IDictionary<string, EntityTagHeaderValue> ETagStore = new Dictionary<string, EntityTagHeaderValue>();
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

        public static void InvalidateETag(CacheKey cacheKey)
        {
            OutputCacheHandler.ETagStore[cacheKey.FullCacheKey] = CreateNewVersion();
        }

        private static EntityTagHeaderValue CreateNewVersion()
        {
            Thread.Sleep(1);
            return new EntityTagHeaderValue(string.Format("\"{0}\"", DateTime.Now.Ticks));
        }

        
        public static EntityTagHeaderValue GetOrCreateETag(CacheKey cacheKey)
        {
            var key = cacheKey.FullCacheKey;
            if (!ETagStore.ContainsKey(key))
            {
                OutputCacheHandler.ETagStore[key] = CreateNewVersion();
            }
            return ETagStore[key];
        }

        internal static EntityTagHeaderValue ETag(CacheKey cacheKey)
        {
            var key = cacheKey.FullCacheKey;
            if (ETagStore.ContainsKey(key))
            {
                return ETagStore[key];
            }

            return null;
        }

        public static void Invalidate(CacheKey cacheKey)
        {
            InvalidateOutputCache(cacheKey);
            InvalidateETag(cacheKey);
        }
    }
}

