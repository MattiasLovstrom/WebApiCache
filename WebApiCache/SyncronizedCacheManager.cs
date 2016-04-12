using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace WebApiCache
{
    public class SynchronizedCacheManager
    {
        public static TimeSpan MaximunUpdateTime = TimeSpan.FromMinutes(10.0);

        public static SynchronizedCacheManager Instance { get; set; }
        private MemoryCache _cache = MemoryCache.Default;
        private IDictionary<string, object> _invalidItems =
            new ConcurrentDictionary<string, object>();

        public event EventHandler<CacheEventArgs> CacheItemRemoved;

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
                }
                else
                {
                    var invalidatedItemCollection = _invalidItems[key.Area] as ConcurrentDictionary<string, DateTime>;
                    if (invalidatedItemCollection != null
                        && invalidatedItemCollection.ContainsKey(key.Key)
                        && invalidatedItemCollection[key.Key].Add(MaximunUpdateTime) < DateTime.UtcNow)
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

        public virtual void RemoveLocal(CacheKey key)
        {
            ReleaseInvalidation(key);
            _cache.Remove(key.FullCacheKey, null);
        }

        //TODO Remove must remove all children if its an area
        public virtual void Remove(CacheKey key)
        {
            RemoveLocal(key);
            OnRemove(new CacheEventArgs() { CacheKey = key, Operation = CacheOperation.Remove });
        }

        public virtual void OnRemove(CacheEventArgs args)
        {
            EventHandler<CacheEventArgs> handler = CacheItemRemoved;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public void Invalidate(CacheKey key)
        {
            if (key.IsArea)
            {
                _invalidItems[key.Area] = DateTime.UtcNow;
            }
            else
            {
                _invalidItems[key.Area] = new ConcurrentDictionary<string, DateTime>();
                ((IDictionary<string, DateTime>)_invalidItems[key.Area])[key.Key] = DateTime.UtcNow;
            }
        }

        public void ReleaseInvalidation(CacheKey key)
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

            if (!cacheKey.IsArea)
            {
                var areaCacheItem = _cache.Get(cacheKey.Area);
                if (areaCacheItem == null)
                {
                    _cache.Set(cacheKey.Area, new Object(), new CacheItemPolicy()
                    {
                        SlidingExpiration = TimeSpan.FromDays(365.0)
                    });
                }

                policy.ChangeMonitors.Add(_cache.CreateCacheEntryChangeMonitor(new[] { cacheKey.Area }));
            }

            ReleaseInvalidation(cacheKey);
            _cache.Set(cacheKey.FullCacheKey, value, policy, null);
        }
    }
}

