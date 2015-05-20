using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using WebApiCache;

namespace CacheWebApiTests
{
    [TestClass]
    public class Versions
    {
        [TestMethod]
        public void ETagInvalidateArea()
        {
            CacheAttribute filter = new CacheAttribute {
                DecalringType = typeof(ExternalInvalidatedController),
                VaryByPath = true
            };
            var url = new Uri("http://epiwiki.se/test");
            Browser browser = new Browser(){Url = url };
            browser.MakeRequest(filter);
        
            ETagStore.Invalidate(new CacheKey(typeof(ExternalInvalidatedController)));
            
            browser = new Browser(browser.ETag){Url = url };
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.ControllerExecuted);
        }

        [TestMethod]
        public void OutputCacheRemoveArea()
        {
            CacheAttribute filter = new CacheAttribute
            {
                DecalringType = typeof(ExternalInvalidatedController),
                VaryByPath = true,
                CacheOnServer = true
            };
            var url = new Uri("http://epiwiki.se/test");
            Browser browser = new Browser(url);
            browser.MakeRequest(filter);

            SynchronizedCacheManager.Instance.Remove(new CacheKey(typeof(ExternalInvalidatedController)));

            browser = new Browser(url);
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.ControllerExecuted);
        }
    }
}

