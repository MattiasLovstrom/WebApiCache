using System;

namespace WebApiCache
{
    public class CacheEventArgs : EventArgs
    {
        public CacheKey CacheKey { get; set; }
        public CacheOperation Operation { get; set; }
    }
}
