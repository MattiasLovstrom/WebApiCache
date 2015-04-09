using System;

namespace WebApiCache
{
    public class WebApiCacheManager
    {
        public static void Invalidate(Type type)
        {
            OutputCacheHandler.Invalidate(type);
            WebApiCacheAttribute.InvalidateETag(type);
        }
    }
}

