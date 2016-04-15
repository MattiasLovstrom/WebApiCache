using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebApiCache
{
    public class HttpRequestMessageWrapper : MessageWrapperBase
    {
        private HttpRequestMessage _httpRequestMessage;

        public HttpRequestMessageWrapper(
            HttpRequestMessage httpRequestMessage)
            : base(httpRequestMessage.RequestUri)
        {
            _httpRequestMessage = httpRequestMessage;
        }

        public HttpRequestMessage Request { get { return _httpRequestMessage; } }


    }
}
