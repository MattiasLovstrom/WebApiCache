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
        private string _cacheKey;
        
        public MessageWrapperBase(Uri uri, Type decalringType, List<string> varyByParam)
        {
            _uri = uri;
            _decalringType = decalringType;
            this._varyByParam = varyByParam;
        }

        public string CurrentCacheKey
        {
            get
            {
                if (_cacheKey == null)
                {
                    _cacheKey = GetCacheKey(
                        _uri,
                        _decalringType,
                        _varyByParam);
                }

                return _cacheKey;
            }
        }

        protected string GetCacheKey(Uri uri, Type DecalringType, List<string> _varyByParam)
        {
            var ret = "WebAPiCache" + DecalringType.FullName;
            if (_varyByParam == null)
            {
                return ret;
            }

            var customCacheKey = new StringBuilder(ret);
            var parameters = uri.ParseQueryString();
            foreach (var param in _varyByParam)
            {
                if (parameters[param] != null)
                {
                    customCacheKey.AppendFormat("{0}={1}", param, parameters[param]);
                }
            }

            return (customCacheKey.ToString());
        }     
    }
}
