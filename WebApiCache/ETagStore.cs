using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using System.Threading;

namespace WebApiCache
{
    public static class ETagStore
    {
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, EntityTagHeaderValue>>
            _eTagStore = new ConcurrentDictionary<string, ConcurrentDictionary<string, EntityTagHeaderValue>>();

        public static void Invalidate(CacheKey cacheKey)
        {
            ConcurrentDictionary<string, EntityTagHeaderValue> areaContainer;
            if (_eTagStore.TryGetValue(cacheKey.Area, out areaContainer))
            {
                if (areaContainer.ContainsKey(cacheKey.Key))
                {
                    areaContainer.AddOrUpdate(
                        cacheKey.Key,
                        CreateNewVersion(),
                        (key, value) => CreateNewVersion());
                }
                else if (cacheKey.IsArea)
                {
                    _eTagStore.AddOrUpdate(
                        cacheKey.Area,
                        new ConcurrentDictionary<string, EntityTagHeaderValue>(),
                        (key, value) => new ConcurrentDictionary<string, EntityTagHeaderValue>());
                }
            }
        }

        private static EntityTagHeaderValue CreateNewVersion()
        {
            Thread.Sleep(1);
            return new EntityTagHeaderValue(string.Format(CultureInfo.InvariantCulture, "\"{0}\"", DateTime.Now.Ticks));
        }

        public static EntityTagHeaderValue GetOrCreateETag(CacheKey cacheKey)
        {
            var areaTags = _eTagStore.GetOrAdd(cacheKey.Area, (key) => new ConcurrentDictionary<string, EntityTagHeaderValue>());
            return areaTags.GetOrAdd(cacheKey.Key, (key) => CreateNewVersion());
        }
    }
}
