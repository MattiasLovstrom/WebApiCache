using System;
using System.Net.Http;

namespace WebApiCache.Rules.ResponseRules
{
    public class AppendParamCacheKeyForResponse : IResponseRule
    {
        private CacheAttribute _attribute;

        public AppendParamCacheKeyForResponse(CacheAttribute attribute)
        {
            _attribute = attribute;
        }

        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            AppendVaryByParamCacheKey(response.CurrentCacheKey, response.Response.RequestMessage.RequestUri, _attribute.GetVarByParams());
            return response;
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
