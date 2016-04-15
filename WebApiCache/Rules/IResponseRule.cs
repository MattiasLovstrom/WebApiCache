namespace WebApiCache.Rules
{
    public interface IResponseRule
    {
        HttpResponseMessageWrapper Invoke(HttpResponseMessageWrapper response);
    }
}