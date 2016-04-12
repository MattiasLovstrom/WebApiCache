using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiCache
{
    public class CacheEventArgs
    {
        public CacheKey CacheKey { get; set; }
        public CacheOperation Operation { get; set; }
    }
}
