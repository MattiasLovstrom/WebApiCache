using System;
using System.Net.Http;

namespace WebApiCache
{
    public class HttpResponseMessageWrapper : MessageWrapperBase
    {
        private HttpResponseMessage _httpResponseMessage;
        
        public HttpResponseMessageWrapper(
            HttpResponseMessage httpResponseMessage)
            : base()
        {
            _httpResponseMessage = httpResponseMessage;
        }

        public HttpResponseMessage Response 
        { 
            get 
            {
                return _httpResponseMessage;
            } 
        }
    }
}
