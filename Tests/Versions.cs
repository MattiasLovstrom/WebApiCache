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
            WebApiCacheAttribute filter = new WebApiCacheAttribute {
                DecalringType = typeof(ExternalInvalidatedController)
            };
            Browser browser = new Browser();
            browser.MakeRequest(filter);
            Assert.IsTrue(!string.IsNullOrEmpty(browser.ActionExecutedContext.Response.Headers.ETag.Tag));
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.OK);
            OutputCacheHandler.Invalidate(new CacheKey(typeof(ExternalInvalidatedController)));
            browser = new Browser(browser.ETag);
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.ControllerExecuted);
        }
    }
}

