using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace WebApiCache
{
    public class SynchronizedCacheManager
    {
        public static readonly TimeSpan MaximunUpdateTime = TimeSpan.FromMinutes(10.0);

        private static MemoryCache _cache = MemoryCache.Default;
        private static IDictionary<string, object> _invalidItems =
            new ConcurrentDictionary<string, object>();

        static SynchronizedCacheManager()
        {
            Instance = new SynchronizedCacheManager();
        }
        
        public virtual object Get(CacheKey key)
        {
            if (_invalidItems.ContainsKey(key.Area))
            {
                if (key.IsArea)
                {
                    var invalidatedArea = _invalidItems[key.Area] as DateTime?;
                    if (invalidatedArea != null
                        && invalidatedArea.Value.Add(MaximunUpdateTime) < DateTime.UtcNow)
                    {
                        _invalidItems.Remove(key.Area); 
                        Remove(key);
                        return null;
                    }
                } else
                {
                    var invalidatedItemCollection =  _invalidItems[key.Area] as ConcurrentDictionary<string, DateTime>; 
                    if (invalidatedItemCollection != null 
                        && invalidatedItemCollection.ContainsKey(key.Key)
                        && invalidatedItemCollection[key.Key].Add(MaximunUpdateTime)< DateTime.UtcNow)
                    {
                        DateTime tmp; 
                        invalidatedItemCollection.TryRemove(key.Key, out tmp);
                        if (invalidatedItemCollection.Count == 0)
                        {
                            _invalidItems[key.Area] = null;
                        }
                        return null;
                    }
                }
            }

            return _cache.Get(key.FullCacheKey, null);
        }

        //TODO Remove must remove all children if its an area
        public virtual void Remove(CacheKey key)
        {
            Release(key);
            _cache.Remove(key.FullCacheKey, null);
        }

        public void Invalidate(CacheKey key)
        {
            if (key.IsArea)
            {
                _invalidItems[key.Area] = DateTime.UtcNow;
            }
            else
            {
                _invalidItems[key.Area] = new ConcurrentDictionary<string, DateTime?>();
                ((IDictionary<string, DateTime>)_invalidItems[key.Area])[key.Key] = DateTime.UtcNow;
            }
        }

        public void Release(CacheKey key)
        {
            if (key.IsArea)
            {
                _invalidItems.Remove(key.Area);
            }
            else 
            {
                if (_invalidItems.ContainsKey(key.Area))
                {
                    ((IDictionary<string, DateTime>)_invalidItems[key.Area]).Remove(key.Key);
                }
            }
        }



        public virtual void Set(CacheKey cacheKey, object value)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromDays(365.0)
            };
            Release(cacheKey);
            _cache.Set(cacheKey.FullCacheKey, value, policy, null);
        }

        public static SynchronizedCacheManager Instance { get; set; }
    }
}

