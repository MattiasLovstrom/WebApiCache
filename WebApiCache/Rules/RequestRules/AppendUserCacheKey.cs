using System.Threading;

namespace WebApiCache.Rules.RequestRules
{
    public class AppendUserCacheKey : IRequestRule
    {
        public HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request)
        {
            request.CurrentCacheKey.Append(Thread.CurrentPrincipal.Identity.Name);
            return null;
        }
    }
}
