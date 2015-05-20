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
        private Type _decalringType;
        private List<string> _varyByParam;
        private CacheKey _cacheKey;
        
        public MessageWrapperBase(Uri uri, Type decalringType, List<string> varyByParam)
        {
            _uri = uri;
            _decalringType = decalringType;
            this._varyByParam = varyByParam;
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
