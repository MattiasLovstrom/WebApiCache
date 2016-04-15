namespace WebApiCache.Rules.RequestRules
{
    public class AppendPathCacheKey : IRequestRule
    {
        public HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request)
        {
            request.CurrentCacheKey.Append(request.Request.RequestUri.AbsolutePath);
            return null;
        }
    }
}
