namespace WebApiCache.Rules.RequestRules
{
    public class TryDeliverFromOutputCache : IRequestRule
    {
        public HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request)
        {
            return SynchronizedCacheManager.Instance.Get(request.CurrentCacheKey) as HttpResponseMessageWrapper;
        }
    }
}
