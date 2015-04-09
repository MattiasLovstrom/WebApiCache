using System;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace WebApiCache
{
    public class CachedResponse
    {
        public CachedResponse(HttpResponseMessage response)
        {
            Response = response;
        }

        public DateTime? Invalidated { get; set; }

        public HttpResponseMessage Response { get; set; }
    }
}

