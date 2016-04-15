using System.Net.Http;

namespace WebApiCache
{
    public class HttpRequestMessageWrapper : MessageWrapperBase
    {
        private HttpRequestMessage _httpRequestMessage;

        public HttpRequestMessageWrapper(
            HttpRequestMessage httpRequestMessage)
            : base()
        {
            _httpRequestMessage = httpRequestMessage;
        }

        public HttpRequestMessage Request { get { return _httpRequestMessage; } }
    }
}
