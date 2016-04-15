namespace WebApiCache.Rules.ResponseRules
{
    public class AppendPathCacheKeyForResponse : IResponseRule
    {
        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            response.CurrentCacheKey.Append(response.Response.RequestMessage.RequestUri.AbsolutePath);
            return response;
        }
    }
}
