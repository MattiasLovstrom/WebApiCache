namespace WebApiCache.Rules.RequestRules
{
    public class InvalidateOutputCache : IRequestRule
    {
        public HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request)
        {
            SynchronizedCacheManager.Instance.Invalidate(request.CurrentCacheKey);
            return null;
        }
    }
}
