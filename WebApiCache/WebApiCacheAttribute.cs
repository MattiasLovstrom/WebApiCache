using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebApiCache
{ 
    public class WebApiCacheAttribute : ActionFilterAttribute
    {
        private bool _isInitialized;
        //TODO Should utalize the cache
        private static IDictionary<Type, EntityTagHeaderValue> ETagStore = new Dictionary<Type, EntityTagHeaderValue>();
        protected IDictionary<string, Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper>> RequestRules { get; set; }
        protected IDictionary<string, Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper>> ResponseRules { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.Response = null;
            if (!_isInitialized)
            {
                Initialize(actionContext);
            }

            var request = new HttpRequestMessageWrapper(actionContext.Request, DecalringType, _varyByParam);
            foreach (Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> func in RequestRules.Values)
            {
                HttpResponseMessageWrapper message = func(request);
                if (message != null)
                {
                    actionContext.Response = message.Response;
                }
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var response = new HttpResponseMessageWrapper(actionExecutedContext.Response, actionExecutedContext.Request.RequestUri, DecalringType, _varyByParam);
            
            foreach (Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> func in ResponseRules.Values)
            {
                func(response);
            }
            base.OnActionExecuted(actionExecutedContext);
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
            RequestRules = new Dictionary<string, Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper>>();
            ResponseRules = new Dictionary<string, Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper>>();
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
            //Updates
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


        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> ApplyETag()
        {
            return delegate (HttpResponseMessageWrapper response) {
                response.Response.Headers.ETag = ETagStore[DecalringType];
                return response;
            };
        }

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> ApplyETagForUser()
        {
            return delegate (HttpResponseMessageWrapper response) {
                response.Response.Headers.ETag = EtagForUser;
                return response;
            };
        }

        private static EntityTagHeaderValue CreateNewVersion()
        {
            Thread.Sleep(1);
            return new EntityTagHeaderValue(string.Format("\"{0}\"", DateTime.Now.Ticks));
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> IfNoneMatch()
        {
            return delegate (HttpRequestMessageWrapper request) {
                if ((request.Request.Method == HttpMethod.Get) 
                    && request.Request.Headers.IfNoneMatch.Contains(ETagStore[DecalringType]))
                {
                    return new HttpResponseMessageWrapper(
                        request.Request.CreateResponse(HttpStatusCode.NotModified),
                        request.Request.RequestUri,
                        DecalringType,
                        _varyByParam);
                }
                return null;
            };
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> IfNoneMatchForUser()
        {
            return delegate (HttpRequestMessageWrapper request) {
                if ((request.Request.Method == HttpMethod.Get) 
                    && request.Request.Headers.IfNoneMatch.Contains(EtagForUser))
                {
                    return new HttpResponseMessageWrapper(
                        request.Request.CreateResponse(HttpStatusCode.NotModified),
                        request.Request.RequestUri,
                        DecalringType,
                        _varyByParam);
                }
                return null;
            };
        }

        

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> InvalidateETag()
        {
            return delegate (HttpResponseMessageWrapper response) {
                InvalidateETag(DecalringType);
                return response;
            };
        }

        public static void InvalidateETag(Type decalringType)
        {
            ETagStore[decalringType] = CreateNewVersion();
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> InvalidateOutputCache()
        {
            return delegate (HttpRequestMessageWrapper request) 
            {
                OutputCacheHandler.Invalidate(request.CurrentCacheKey);
                return null;
            };
        }

        

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> SetPublicCacheHeaders()
        {
            return delegate (HttpResponseMessageWrapper response) {
                CacheControlHeaderValue value2 = new CacheControlHeaderValue {
                    MaxAge = new TimeSpan?(TimeSpan.Zero),
                    Public = true,
                    MustRevalidate = true
                };
                response.Response.Headers.CacheControl = value2;
                return response;
            };
        }

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> StoreInOutputCache()
        {
            return delegate (HttpResponseMessageWrapper response) {
                OutputCacheHandler.Set(response.CurrentCacheKey, response);
                return response;
            };
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> TryDeliverFromOutputCache()
        {
            return request => OutputCacheHandler.Get(request.CurrentCacheKey);
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

        
        public virtual bool Update { get; set; }

        public bool VaryByUser { get; set; }

        private List<String> _varyByParam;
        public string VarByParam { 
            set 
            {
                _varyByParam = new List<string>(value.Split(new[] { ',' }));
            }
        }
    }
}

