using System;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace WebApiCache
{
    public class CachedResponse
    {
        public CachedResponse(HttpResponseMessageWrapper response)
        {
            Response = response;
        }

        public DateTime? Invalidated { get; set; }

        public HttpResponseMessageWrapper Response { get; set; }
    }
}

