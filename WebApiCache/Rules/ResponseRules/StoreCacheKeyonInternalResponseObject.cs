namespace WebApiCache.Rules.ResponseRules
{
    public class StoreCacheKeyonInternalResponseObject : IResponseRule
    {
        CacheAttribute attribute;
        public StoreCacheKeyonInternalResponseObject(CacheAttribute attribute)
        {
            this.attribute = attribute;
        }

        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            response.CurrentCacheKey = new CacheKey(attribute.DecalringType);
            return response;
        }
    }
}
