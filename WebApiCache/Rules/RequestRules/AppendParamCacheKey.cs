using System;
using System.Net.Http;

namespace WebApiCache.Rules.RequestRules
{
    public class AppendParamCacheKey : IRequestRule
    {
        private CacheAttribute _attribute;
        public AppendParamCacheKey(CacheAttribute attribute)
        {
            _attribute = attribute;
        }

        public HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request)
        {
            AppendVaryByParamCacheKey(request.CurrentCacheKey, request.Request.RequestUri, _attribute.GetVarByParams());
            return null;
        }

        protected void AppendVaryByParamCacheKey(CacheKey currentCacheKey, Uri uri, string[] _varyByParam)
        {
            var parameters = uri.ParseQueryString();
            foreach (var param in _varyByParam)
            {
                if (parameters[param] != null)
                {
                    currentCacheKey.Append(String.Format("{0}={1}", param, parameters[param]));
                }
            }
        }

    }
}
