using System;
using System.Net.Http;

namespace WebApiCache
{
    public class HttpResponseMessageWrapper : MessageWrapperBase
    {
        private HttpResponseMessage _httpResponseMessage;
        private Type _decalringType;
        private string[] _varyByParam;
        
        public HttpResponseMessageWrapper(
            HttpResponseMessage httpResponseMessage,
            Uri uri)
            : base(uri)
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

        public DateTime? Invalidated { get; set; }
    }
}
