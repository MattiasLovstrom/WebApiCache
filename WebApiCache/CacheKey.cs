using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiCache
{
    public class CacheKey
    {
        private StringBuilder _key;
        public CacheKey(string area, string key)
        {
            Area = area;
            _key = new StringBuilder(key);
        }

        public CacheKey(Type DecalringType)
        {
            Area = DecalringType.FullName;
        }
        public string Area { get; set;}
        public string Key 
        {
            get 
            {
                return _key == null ? "" : _key.ToString();
            }
            set
            {
                _key = new StringBuilder(value);
            }
        }
        public string FullCacheKey { get { return Area + Key; } }

        public void Append(string key)
        {
            if (_key == null)
            {
                _key = new StringBuilder();
            }
            _key.Append(key);
        }

        public bool IsArea { get { return _key == null; } }
    }
}
