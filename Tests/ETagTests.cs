using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using WebApiCache;

namespace CacheWebApiTests
{
    [TestClass]
    public class ETagTests
    {
        [TestMethod]
        public void ETagCacheVaryByParams()
        {
            ServerCache.ClearCache();

            CacheAttribute readController = new CacheAttribute
            {
                VarByParam="id,lang"
            };
            CacheAttribute updateController = new CacheAttribute
            {
                Update = true,
                VarByParam="id,lang"
            };

            Uri uri1 = new Uri("http://epiwiki.se/?id=1&lang=sv");
            Uri uri2 = new Uri("http://epiwiki.se/?id=2&lang=sv");
            Uri uri3 = new Uri("http://epiwiki.se/?id=2&lang=sv&notincluded=true");
           
            // first url should get a etag
            var browser1 = new Browser(uri1);
            browser1.MakeRequest(readController);

            // second url should get another etag
            var browser2 = new Browser(uri2);
            browser2.MakeRequest(readController);
            Assert.AreNotEqual(browser1.ETag, browser2.ETag);

            // second url should get another etag
            var browser3 = new Browser(uri3);
            browser3.MakeRequest(readController);
            Assert.AreEqual(browser2.ETag, browser3.ETag);
        }
        
        [TestMethod]
        public void ETagCacheVaryByPath()
        {
            // /api/bubble/00000000-0000-0000-0000-000000000000/messages/
            // /api/bubble/00000000-0000-0000-0000-000000000001/messages/

            CacheAttribute readController = new CacheAttribute
            {
                VaryByPath = true
            };
            CacheAttribute updateController = new CacheAttribute
            {
                Update = true,
                VaryByPath = true
            };

            Uri uri1 = new Uri("http://epiwiki.se/api/bubble/00000000-0000-0000-0000-000000000000/messages/");
            Uri uri2 = new Uri("http://epiwiki.se/api/bubble/00000000-0000-0000-0000-000000000001/messages/");

            // first request should load the cache 
            var browser1 = new Browser(uri1);
            browser1.MakeRequest(readController);
            // The second request should read from cace
            Browser browser2 = new Browser(browser1.ETag)
            {
                Url = uri1
            };
            browser2.MakeRequest(readController);
            Assert.IsFalse(browser2.ControllerExecuted);

            //Another url should load the cache 
            Browser browser3 = new Browser(browser1.ETag)
            {
                Url = uri2
            };
            browser3.MakeRequest(readController);
            Assert.IsTrue(browser3.ControllerExecuted);

            //browser1.MakeRequest(readController);
            //Assert.IsTrue(browser1.ControllerExecuted);
        }

        [TestMethod]
        public void ETagDeclarentType()
        {
            CacheAttribute filter = new CacheAttribute();
            CacheAttribute attribute2 = new CacheAttribute 
            {
                DecalringType = typeof(TestController)
            };
            Browser browser = new Browser();
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.OK);
            Browser browser2 = new Browser(browser.ETag) 
            {
                ControllerType = typeof(TestController2)
            };
            browser2.MakeRequest(attribute2);
            Assert.IsTrue(browser2.StatusCode == HttpStatusCode.NotModified);
        }

        [TestMethod]
        public void ETagFullRequest()
        {
            CacheAttribute filter = new CacheAttribute();
            Browser browser = new Browser();
            browser.MakeRequest(filter);
            Assert.IsTrue(!string.IsNullOrEmpty(browser.ActionExecutedContext.Response.Headers.ETag.Tag));
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.OK);
            browser = new Browser(browser.ETag);
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.NotModified);
        }

        [TestMethod]
        public void ETagNotUpdate()
        {
            CacheAttribute filter = new CacheAttribute();
            Browser browser = new Browser();
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.ETag != null);
            browser = new Browser(browser.ETag);
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.NotModified);
        }

        [TestMethod]
        public void ETagUpdate()
        {
            CacheAttribute filter = new CacheAttribute {
                Update = true
            };
            Browser browser = new Browser();
            browser.MakeRequest(filter);
            Assert.IsTrue(browser.ETag != null);
            EntityTagHeaderValue eTag = browser.ETag;
            browser = new Browser(eTag);
            browser.MakeRequest(filter);
            Assert.AreNotEqual<EntityTagHeaderValue>(browser.ETag, eTag);
            Assert.IsTrue(browser.StatusCode == HttpStatusCode.OK);
        }

        [TestMethod]
        public void ETagUpdateForUser()
        {
            CacheAttribute filter = new CacheAttribute {
                VaryByUser = true
            };
            CacheAttribute updateFilter = new CacheAttribute {
                VaryByUser = true,
                Update = true
            };
            IPrincipal principal = new GenericPrincipal(new GenericIdentity("user1"), new string[] { "role1", "role2" });
            IPrincipal principal2 = new GenericPrincipal(new GenericIdentity("user2"), new string[] { "role1", "role3" });
            Thread.CurrentPrincipal = principal;
            Browser browser = new Browser();
            //First request update cace for principal1
            browser.MakeRequest(filter);
            Thread.CurrentPrincipal = principal2;
            Browser browser2 = new Browser();
            //First request update cache for principal2
            browser2.MakeRequest(filter);
            EntityTagHeaderValue eTag = browser2.ETag;
            Assert.AreNotEqual<EntityTagHeaderValue>(browser.ETag, browser2.ETag);
            //Make Update should invalidate for all since it not marked VaryByUser
            browser2.MakeRequest(updateFilter);
            //Make normal request 
            browser2.MakeRequest(filter);
            Assert.AreNotEqual<EntityTagHeaderValue>(eTag, browser2.ETag);
        }
    }
}

