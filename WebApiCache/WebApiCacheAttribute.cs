using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebApiCache
{
    public class WebApiCacheAttribute : ActionFilterAttribute
    {
        private bool _isInitialized;
        private static IDictionary<Type, EntityTagHeaderValue> ETagStore = new Dictionary<Type, EntityTagHeaderValue>();

        private Func<HttpResponseMessage, HttpResponseMessage> ApplyETag()
        {
            return delegate (HttpResponseMessage response) {
                response.Headers.ETag = ETagStore[DecalringType];
                return response;
            };
        }

        private Func<HttpResponseMessage, HttpResponseMessage> ApplyETagForUser()
        {
            return delegate (HttpResponseMessage response) {
                response.Headers.ETag = EtagForUser;
                return response;
            };
        }

        private static EntityTagHeaderValue CreateNewVersion()
        {
            Thread.Sleep(1);
            return new EntityTagHeaderValue(string.Format("\"{0}\"", DateTime.Now.Ticks));
        }

        private Func<HttpRequestMessage, HttpResponseMessage> IfNoneMatch()
        {
            return delegate (HttpRequestMessage request) {
                if ((request.Method == HttpMethod.Get) && request.Headers.IfNoneMatch.Contains(ETagStore[DecalringType]))
                {
                    return request.CreateResponse(HttpStatusCode.NotModified);
                }
                return null;
            };
        }

        private Func<HttpRequestMessage, HttpResponseMessage> IfNoneMatchForUser()
        {
            return delegate (HttpRequestMessage request) {
                if ((request.Method == HttpMethod.Get) && request.Headers.IfNoneMatch.Contains(EtagForUser))
                {
                    return request.CreateResponse(HttpStatusCode.NotModified);
                }
                return null;
            };
        }

        private void Initialize(HttpActionContext actionContext)
        {
            _isInitialized = true;
            if (DecalringType == null)
            {
                DecalringType = actionContext.ControllerContext.ControllerDescriptor.ControllerType;
            }
            if (!ETagStore.ContainsKey(DecalringType))
            {
                ETagStore.Add(DecalringType, CreateNewVersion());
            }
            RequestRules = new Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>>();
            ResponseRules = new Dictionary<string, Func<HttpResponseMessage, HttpResponseMessage>>();
            if (!Update)
            {
                if (VaryByUser)
                {
                    RequestRules.Add("IfNoneMatchForUser", IfNoneMatchForUser());
                }
                else
                {
                    RequestRules.Add("IfNoneMatch", IfNoneMatch());
                    if (CacheOnServer)
                    {
                        RequestRules.Add("TryDeliverFromOutputCache", TryDeliverFromOutputCache());
                    }
                }
            }
            else
            {
                if (CacheOnServer)
                {
                    RequestRules.Add("InvalidateOutputCache", InvalidateOutputCache());
                }
                ResponseRules.Add("Invalidate", InvalidateETag());
            }
            if (VaryByUser)
            {
                ResponseRules.Add("ApplyETagForUser", ApplyETagForUser());
            }
            else
            {
                ResponseRules.Add("ApplyETag", ApplyETag());
                if (CacheOnServer)
                {
                    ResponseRules.Add("StoreInOutputCache", StoreInOutputCache());
                }
            }
            ResponseRules.Add("SetPublicCacheHeaders", SetPublicCacheHeaders());
        }

        private Func<HttpResponseMessage, HttpResponseMessage> InvalidateETag()
        {
            return delegate (HttpResponseMessage response) {
                InvalidateETag(DecalringType);
                return response;
            };
        }

        public static void InvalidateETag(Type decalringType)
        {
            ETagStore[decalringType] = CreateNewVersion();
        }

        private Func<HttpRequestMessage, HttpResponseMessage> InvalidateOutputCache()
        {
            return delegate (HttpRequestMessage response) {
                OutputCacheHandler.Invalidate(DecalringType);
                return null;
            };
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            foreach (Func<HttpResponseMessage, HttpResponseMessage> func in ResponseRules.Values)
            {
                func(actionExecutedContext.Response);
            }
            base.OnActionExecuted(actionExecutedContext);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.Response = null;
            if (!_isInitialized)
            {
                Initialize(actionContext);
            }
            foreach (Func<HttpRequestMessage, HttpResponseMessage> func in RequestRules.Values)
            {
                HttpResponseMessage message = func(actionContext.Request);
                if (message != null)
                {
                    actionContext.Response = message;
                }
            }
        }

        private Func<HttpResponseMessage, HttpResponseMessage> SetPublicCacheHeaders()
        {
            return delegate (HttpResponseMessage response) {
                CacheControlHeaderValue value2 = new CacheControlHeaderValue {
                    MaxAge = new TimeSpan?(TimeSpan.Zero),
                    Public = true,
                    MustRevalidate = true
                };
                response.Headers.CacheControl = value2;
                return response;
            };
        }

        private Func<HttpResponseMessage, HttpResponseMessage> StoreInOutputCache()
        {
            return delegate (HttpResponseMessage response) {
                OutputCacheHandler.Set(DecalringType, response);
                return response;
            };
        }

        private Func<HttpRequestMessage, HttpResponseMessage> TryDeliverFromOutputCache()
        {
            return request => OutputCacheHandler.Get(DecalringType);
        }

        public bool CacheOnServer { get; set; }

        public Type DecalringType { get; set; }

        public EntityTagHeaderValue EtagForUser
        {
            get
            {
                string str = ETagStore[DecalringType].Tag.Trim(new char[] { '"' });
                return new EntityTagHeaderValue(string.Format("\"{0}_{1}\"", str, Thread.CurrentPrincipal.Identity.Name));
            }
        }

        public virtual Type Key { get; set; }

        protected IDictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> RequestRules { get; set; }

        protected IDictionary<string, Func<HttpResponseMessage, HttpResponseMessage>> ResponseRules { get; set; }

        public virtual bool Update { get; set; }

        public bool VaryByUser { get; set; }
    }
}

