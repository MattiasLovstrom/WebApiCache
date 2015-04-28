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
        public void OutputcacheExternalInvalidation()
        {
            CacheAttribute filter = new CacheAttribute {
                DecalringType = typeof(ExternalInvalidatedController)
            };
            Browser browser = new Browser();
            browser.MakeRequest(filter);
            Assert.IsTrue(!string.IsNullOrEmpty(browser.ActionExecutedContext.Response.Headers.ETag.Tag));
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.OK);
            WebApiCacheManager.Invalidate(typeof(ExternalInvalidatedController));
            browser = new Browser(browser.ETag);
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.ControllerExecuted);
        }
    }
}

