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
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ETagDeclarentType()
        {
            WebApiCacheAttribute filter = new WebApiCacheAttribute();
            WebApiCacheAttribute attribute2 = new WebApiCacheAttribute 
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
            WebApiCacheAttribute filter = new WebApiCacheAttribute();
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
            WebApiCacheAttribute filter = new WebApiCacheAttribute();
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
            WebApiCacheAttribute filter = new WebApiCacheAttribute {
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
            WebApiCacheAttribute filter = new WebApiCacheAttribute {
                VaryByUser = true
            };
            WebApiCacheAttribute attribute2 = new WebApiCacheAttribute {
                Update = true
            };
            IPrincipal principal = new GenericPrincipal(new GenericIdentity("user1"), new string[] { "role1", "role2" });
            IPrincipal principal2 = new GenericPrincipal(new GenericIdentity("user2"), new string[] { "role1", "role3" });
            Thread.CurrentPrincipal = principal;
            Browser browser = new Browser();
            browser.MakeRequest(filter);
            Thread.CurrentPrincipal = principal2;
            Browser browser2 = new Browser();
            browser2.MakeRequest(filter);
            EntityTagHeaderValue eTag = browser2.ETag;
            Assert.AreNotEqual<EntityTagHeaderValue>(browser.ETag, browser2.ETag);
            browser2.MakeRequest(attribute2);
            browser2.MakeRequest(filter);
            Assert.AreNotEqual<EntityTagHeaderValue>(eTag, browser2.ETag);
        }
    }
}

