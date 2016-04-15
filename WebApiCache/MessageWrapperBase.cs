using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebApiCache
{
    public class MessageWrapperBase
    {
        private Uri _uri;
        private CacheKey _cacheKey;
        
        public MessageWrapperBase(Uri uri)
        {
            _uri = uri;
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
