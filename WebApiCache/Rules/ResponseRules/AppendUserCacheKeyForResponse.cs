using System.Threading;

namespace WebApiCache.Rules.ResponseRules
{
    public class AppendUserCacheKeyForResponse : IResponseRule
    {
        public HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response)
        {
            response.CurrentCacheKey.Append(Thread.CurrentPrincipal.Identity.Name);
            return response;
        }
    }
}
