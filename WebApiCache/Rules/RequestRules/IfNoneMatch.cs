using System.Net;
using System.Net.Http;

namespace WebApiCache.Rules.RequestRules
{
    public class IfNoneMatch : IRequestRule
    {
        public HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request)
        {
            if (request.Request.Method != HttpMethod.Get
             || !request.Request.Headers.IfNoneMatch.Contains(ETagStore.GetOrCreateETag(request.CurrentCacheKey)))
            {
                return null;
            }

            return new HttpResponseMessageWrapper(
                        request.Request.CreateResponse(HttpStatusCode.NotModified), request.Request.RequestUri);
        }
    }
}
