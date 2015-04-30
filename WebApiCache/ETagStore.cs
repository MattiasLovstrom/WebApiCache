using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApiCache
{
    public class ETagStore
    {
        private static IDictionary<string, IDictionary<string, EntityTagHeaderValue>>
            _eTagStore = new Dictionary<string, IDictionary<string, EntityTagHeaderValue>>();

        public static void Invalidate(CacheKey cacheKey)
        {
            if (_eTagStore.ContainsKey(cacheKey.Area))
            {
                if(_eTagStore[cacheKey.Area].ContainsKey(cacheKey.Key))
                {
                    _eTagStore[cacheKey.Area][cacheKey.Key] = CreateNewVersion();
                } else if(cacheKey.IsArea)
                {
                    _eTagStore[cacheKey.Area] = new Dictionary<string, EntityTagHeaderValue>();
                }
            }
        }

        private static EntityTagHeaderValue CreateNewVersion()
        {
            Thread.Sleep(1);
            return new EntityTagHeaderValue(string.Format("\"{0}\"", DateTime.Now.Ticks));
        }

        public static EntityTagHeaderValue GetOrCreateETag(CacheKey cacheKey)
        {
            if (!_eTagStore.ContainsKey(cacheKey.Area))
            {
                _eTagStore[cacheKey.Area] = new Dictionary<string, EntityTagHeaderValue>();
            }

            if (!_eTagStore[cacheKey.Area].ContainsKey(cacheKey.Key))
            {
                _eTagStore[cacheKey.Area][cacheKey.Key] = CreateNewVersion();
            }

            return _eTagStore[cacheKey.Area][cacheKey.Key];
        }

        internal static EntityTagHeaderValue Get(CacheKey cacheKey)
        {
            if (_eTagStore.ContainsKey(cacheKey.Area))
            {
                var area = _eTagStore[cacheKey.Area];
                if (area.ContainsKey(cacheKey.Key))
                {
                    return area[cacheKey.Key];
                }
            }

            return null;
        }
    }
}
