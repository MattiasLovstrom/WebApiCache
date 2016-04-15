namespace WebApiCache.Rules.RequestRules
{
    public class StoreCacheKeyOnInternalRequestObject : IRequestRule
    {
        private CacheAttribute cacheAttribute;

        public StoreCacheKeyOnInternalRequestObject(CacheAttribute cacheAttribute)
        {
            this.cacheAttribute = cacheAttribute;
        }

        public HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request)
        {
            request.CurrentCacheKey = new CacheKey(cacheAttribute.DeclaringType);
            return null;
        }
    }
}
