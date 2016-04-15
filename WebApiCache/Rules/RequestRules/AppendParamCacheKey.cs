using System;
using System.Globalization;
using System.Net.Http;

namespace WebApiCache.Rules.RequestRules
{
    /// <summary>
    /// Appends all request parameters and its value that are defined in varyByParam to the cache key. 
    /// </summary>
    public class AppendParamCacheKey : IRequestRule
    {
        private string[] _varyByParams;
        public AppendParamCacheKey(string[] varyByParams)
        {
            _varyByParams = varyByParams;
        }

        public HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request)
        {
            AppendVaryByParamsCacheKey(request.CurrentCacheKey, request.Request.RequestUri);
            return null;
        }

        private void AppendVaryByParamsCacheKey(CacheKey currentCacheKey, Uri uri)
        {
            var parameters = uri.ParseQueryString();
            foreach (var param in _varyByParams)
            {
                if (parameters[param] != null)
                {
                    currentCacheKey.Append(String.Format(CultureInfo.InvariantCulture, "{0}={1}", param, parameters[param]));
                }
            }
        }

    }
}
