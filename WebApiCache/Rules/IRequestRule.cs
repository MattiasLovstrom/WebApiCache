using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiCache.Rules
{
    public interface IRequestRule
    {
        HttpResponseMessageWrapper Invoke(HttpRequestMessageWrapper request);
    }
}
