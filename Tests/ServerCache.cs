using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using WebApiCache;
using System.Linq;

namespace CacheWebApiTests
{
    [TestClass]
    public class ServerCache
    {
        [TestMethod]
        public void OutputCacheDeliverCachedDuringUpdate()
        {
            ClearCache();

            WebApiCacheAttribute filter = new WebApiCacheAttribute {
                CacheOnServer = true
            };
            WebApiCacheAttribute attribute2 = new WebApiCacheAttribute {
                CacheOnServer = true,
                Update = true
            };
            new Browser().MakeRequest(filter);
            Browser browser = new Browser();
            attribute2.OnActionExecuting(browser.ActionExecutingContext);
            Browser browser2 = new Browser();
            browser2.MakeRequest(filter);
            Assert.IsFalse(browser2.ControllerExecuted);
            EntityTagHeaderValue eTag = browser2.ETag;
            attribute2.OnActionExecuted(browser.ActionExecutedContext);
            browser2 = new Browser();
            browser2.MakeRequest(filter);
            Assert.IsFalse(browser2.ControllerExecuted);
            Assert.AreNotSame(eTag, browser2.ETag);
        }

        [TestMethod]
        public void OutputCacheSimple()
        {
            ClearCache();
            
            WebApiCacheAttribute filter = new WebApiCacheAttribute {
                CacheOnServer = true
            };
            Browser browser = new Browser();
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(browser.ControllerExecuted);
            Browser browser2 = new Browser();
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.OK);
            Assert.IsFalse(browser.ControllerExecuted);
        }

        private static void ClearCache()
        {
            MemoryCache.Default.ToList().ForEach(a => MemoryCache.Default.Remove(a.Key));
        }

        [TestMethod]
        public void OutputCacheVaryByParams()
        {
            throw new NotImplementedException();
        }
    }
}

