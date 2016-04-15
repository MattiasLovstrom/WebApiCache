using System;
using System.Net.Http.Headers;

namespace WebApiCache.Rules.ResponseRules
{
    public class SetPublicCacheHeaders : IResponseRule
    {
        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            CacheControlHeaderValue value2 = new CacheControlHeaderValue
            {
                MaxAge = new TimeSpan?(TimeSpan.Zero),
                Public = true,
                MustRevalidate = true
            };
            if (response.Response != null && response.Response.Headers != null)
            {
                response.Response.Headers.CacheControl = value2;
            }
            return response;
        }
    }
}
