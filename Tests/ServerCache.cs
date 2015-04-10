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

            WebApiCacheAttribute readController = new WebApiCacheAttribute {
                CacheOnServer = true
            };
            WebApiCacheAttribute updateController = new WebApiCacheAttribute {
                CacheOnServer = true,
                Update = true
            };

            // first browser should load the cache 
            new Browser().MakeRequest(readController);
 
            // The second request flag the cache for updates
            Browser updateBrowser = new Browser();
            updateController.OnActionExecuting(updateBrowser.ActionExecutingContext);

            // the third request should deliver old content from cache
            Browser browser2 = new Browser();
            browser2.MakeRequest(readController);
            Assert.IsFalse(browser2.ControllerExecuted);

            //Make the request
            updateBrowser.ExecuteControllerAction(readController);
            // The second request updates the cache
            updateController.OnActionExecuted(updateBrowser.ActionExecutedContext);
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
            browser2.MakeRequest(filter);
            Assert.IsTrue(browser2.StatusCode == HttpStatusCode.OK);
            Assert.IsFalse(browser2.ControllerExecuted);
        }

        private static void ClearCache()
        {
            MemoryCache.Default.ToList().ForEach(a => MemoryCache.Default.Remove(a.Key));
        }

        [TestMethod]
        public void OutputCacheVaryByParams()
        {
            ClearCache();

            WebApiCacheAttribute readController = new WebApiCacheAttribute
            {
                CacheOnServer = true,
                VarByParam="id,lang"
            };
            WebApiCacheAttribute updateController = new WebApiCacheAttribute
            {
                CacheOnServer = true,
                Update = true,
                VarByParam="id,lang"
            };

            Uri uri1 = new Uri("http://epiwiki.se/?id=1&lang=sv");
            Uri uri2 = new Uri("http://epiwiki.se/?id=2&lang=sv");
           
            // first browser should load the cache 
            new Browser(uri1).MakeRequest(readController);

            // The second request should read from cace
            Browser browser = new Browser(uri1);
            browser.MakeRequest(readController);
            Assert.IsFalse(browser.ControllerExecuted);

            // The first request to the other id should update the cace
            Browser browser1 = new Browser(uri2);
            browser1.MakeRequest(readController);
            Assert.IsTrue(browser1.ControllerExecuted);
        }
    }
}

