namespace WebApiCache.Rules.ResponseRules
{
    public class InvalidateETag : IResponseRule
    {
        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            ETagStore.Invalidate(response.CurrentCacheKey);
            return response;
        }
    }
}
