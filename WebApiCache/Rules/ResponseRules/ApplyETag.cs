namespace WebApiCache.Rules.ResponseRules
{
    public class ApplyETag : IResponseRule
    {
        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            if (response != null && response.Response != null && response.Response.Headers != null)
            {
                response.Response.Headers.ETag = ETagStore.GetOrCreateETag(response.CurrentCacheKey);
            }
            return response;
        }
    }
}
