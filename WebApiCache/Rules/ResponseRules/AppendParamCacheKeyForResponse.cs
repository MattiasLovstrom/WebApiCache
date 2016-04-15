using System;
using System.Globalization;
using System.Net.Http;

namespace WebApiCache.Rules.ResponseRules
{
    public class AppendParamsCacheKeyForResponse : IResponseRule
    {
        private string[] _varyByParams;

        public AppendParamsCacheKeyForResponse(string[] varyByParams)
        {
            _varyByParams = varyByParams;
        }

        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            AppendVaryByParamsCacheKey(response.CurrentCacheKey, response.Response.RequestMessage.RequestUri);
            return response;
        }

        protected void AppendVaryByParamsCacheKey(CacheKey currentCacheKey, Uri uri)
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
