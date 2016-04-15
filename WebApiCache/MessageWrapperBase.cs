using System;

namespace WebApiCache
{
    public class MessageWrapperBase
    {
        private CacheKey _cacheKey;
        
        public MessageWrapperBase()
        {
        }

        public CacheKey CurrentCacheKey
        {
            get
            {
                return _cacheKey;
            }
            set
            {
                _cacheKey = value;
            }
        }
    }
}
