using System;
using System.Globalization;
using System.Text;

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

        public CacheKey(Type decalringType)
        {
            Area = decalringType.FullName;
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

        public string Serialize()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}|{1}", Area, Key);  
        }

        static public CacheKey Deserialize(string serialized)
        {
            var split = serialized.Split(new [] { '|' });
            return new CacheKey(
                split[0],
                split.Length > 1 ? split[1] : null);
        } 
    }
}
