using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebApiCache
{
    public class HttpResponseMessageWrapper : MessageWrapperBase
    {
        private HttpResponseMessage _httpResponseMessage;
        private Type _decalringType;
        private List<string> _varyByParam;
        private HttpResponseMessage httpResponseMessage;

        
        public HttpResponseMessageWrapper(
            HttpResponseMessage httpResponseMessage,
            Uri uri,
            Type DecalringType, 
            List<string> varyByParam)
            : base(uri, DecalringType, varyByParam)
        {
            _httpResponseMessage = httpResponseMessage;
            _decalringType = DecalringType;
            _varyByParam = varyByParam;
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
