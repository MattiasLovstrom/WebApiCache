namespace WebApiCache.Rules.ResponseRules
{
    public class StoreInOutputCache : IResponseRule
    {
        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            SynchronizedCacheManager.Instance.Set(response.CurrentCacheKey, response);
            return response;

        }
    }
}
