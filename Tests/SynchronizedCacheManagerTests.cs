using CacheWebApiTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApiCache;

namespace Tests
{
    [TestClass]
    public class SynchronizedCacheManagerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ServerCache.ClearCache();
            SynchronizedCacheManager.Instance = new SynchronizedCacheManager();
        }

        [TestMethod]
        public void CacheSetAndGet()
        {
            var cacheKey = new CacheKey("area", "key");
            object inData = new object();
            SynchronizedCacheManager.Instance.Set(cacheKey, inData);
            object outData = SynchronizedCacheManager.Instance.Get(cacheKey);

            Assert.AreEqual(inData, outData);
        }

        [TestMethod]
        public void CacheInvalidate()
        {
            var cacheKey = new CacheKey("area", "key");
            object inData = new object();
            SynchronizedCacheManager.Instance.Set(cacheKey, inData);
            // Invalidate only indicate this cache object is invalid
            SynchronizedCacheManager.Instance.Invalidate(cacheKey);
            // the same object should be returned
            object outData = SynchronizedCacheManager.Instance.Get(cacheKey);

            Assert.AreEqual(inData, outData);
        }


        [TestMethod]
        public void CacheInvalidateTimeOut()
        {
            var cacheKey = new CacheKey("area", "key");
            object inData = new object();
            SynchronizedCacheManager.MaximumUpdateTime = TimeSpan.FromMilliseconds(1);
            SynchronizedCacheManager.Instance.Set(cacheKey, inData);
            SynchronizedCacheManager.Instance.Invalidate(cacheKey);
            Thread.Sleep(1);
            object outData = SynchronizedCacheManager.Instance.Get(cacheKey);

            Assert.AreEqual(null, outData);
        }

        [TestMethod]
        public void CacheInvalidateWithSet()
        {
            var cacheKey = new CacheKey("area", "key");
            object inData1 = new object();
            object inData2 = new object();
            SynchronizedCacheManager.Instance.Set(cacheKey, inData1);
            SynchronizedCacheManager.Instance.Invalidate(cacheKey);
            object outData1 = SynchronizedCacheManager.Instance.Get(cacheKey);
            SynchronizedCacheManager.Instance.Set(cacheKey, inData2);
            object outData2 = SynchronizedCacheManager.Instance.Get(cacheKey);

            Assert.AreEqual(inData1, outData1);
            Assert.AreEqual(inData2, outData2);
        }
    }
}
