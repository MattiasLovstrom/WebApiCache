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

            RequestRules = new Dictionary<string, Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper>>();
            ResponseRules = new Dictionary<string, Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper>>();

            RequestRules.Add("AddCacheKey", AddCacheKey());
            ResponseRules.Add("AddCacheKey", AddCacheKeyForResponse());
            if (VaryByUser)
            {
                RequestRules.Add("AppendUserCacheKey", AppendUserCacheKey());
                ResponseRules.Add("AppendUserCacheKey", AppendUserCacheKeyForResponse());
            }
            if (VaryByPath)
            {
                RequestRules.Add("AppendPathCacheKey", AppendPathCacheKey());
                ResponseRules.Add("AppendPathCacheKey", AppendPathCacheKeyForResponse());
            }
            if (_varyByParam != null)
            {
                RequestRules.Add("AppendParamCacheKey", AppendParamCacheKey());
                ResponseRules.Add("AppendParamCacheKey", AppendParamCacheKeyForResponse());
            }

            if (!Update)
            {
                RequestRules.Add("IfNoneMatch", IfNoneMatch());
                if (CacheOnServer)
                {
                    RequestRules.Add("TryDeliverFromOutputCache", TryDeliverFromOutputCache());
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

            ResponseRules.Add("ApplyETag", ApplyETag());
            if (CacheOnServer)
            {
                ResponseRules.Add("StoreInOutputCache", StoreInOutputCache());
            }
            ResponseRules.Add("SetPublicCacheHeaders", SetPublicCacheHeaders());
        }

        protected void AppendVaryByParamCacheKey(CacheKey currentCacheKey, Uri uri, List<string> _varyByParam)
        {
            var parameters = uri.ParseQueryString();
            foreach (var param in _varyByParam)
            {
                if (parameters[param] != null)
                {
                    currentCacheKey.Append(String.Format("{0}={1}", param, parameters[param]));
                }
            }
        }

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> AppendParamCacheKeyForResponse()
        {
            return delegate(HttpResponseMessageWrapper response)
            {
                AppendVaryByParamCacheKey(response.CurrentCacheKey, response.Response.RequestMessage.RequestUri, _varyByParam);
                return response;
            };
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> AppendParamCacheKey()
        {
            return delegate(HttpRequestMessageWrapper request)
            {
                AppendVaryByParamCacheKey(request.CurrentCacheKey, request.Request.RequestUri, _varyByParam);
                return null;
            };
        }


        private CacheKey GetCacheKey()
        {
            return new CacheKey(DecalringType);
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> AddCacheKey()
        {
            return delegate(HttpRequestMessageWrapper request)
            {
                request.CurrentCacheKey = GetCacheKey();
                return null;
            };
        }

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> AddCacheKeyForResponse()
        {
            return delegate(HttpResponseMessageWrapper response)
            {
                response.CurrentCacheKey = GetCacheKey();
                return response;
            };
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> AppendPathCacheKey()
        {
            return delegate(HttpRequestMessageWrapper request)
            {
                request.CurrentCacheKey.Append(request.Request.RequestUri.AbsolutePath);
                return null;
            };
        }

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> AppendUserCacheKeyForResponse()
        {
            return delegate(HttpResponseMessageWrapper response)
            {
                response.CurrentCacheKey.Append(Thread.CurrentPrincipal.Identity.Name);
                return response;
            };
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> AppendUserCacheKey()
        {
            return delegate(HttpRequestMessageWrapper request)
            {
                //TODO Fix this
                request.CurrentCacheKey.Append(Thread.CurrentPrincipal.Identity.Name);
                return null;
            };
        }

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> AppendPathCacheKeyForResponse()
        {
            return delegate(HttpResponseMessageWrapper response)
            {
                response.CurrentCacheKey.Append(response.Response.RequestMessage.RequestUri.AbsolutePath);
                return response;
            };
        }

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> ApplyETag()
        {
            return delegate(HttpResponseMessageWrapper response)
            {
                response.Response.Headers.ETag = OutputCacheHandler.GetOrCreateETag(response.CurrentCacheKey);
                return response;
            };
        }




        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> IfNoneMatch()
        {
            return delegate(HttpRequestMessageWrapper request)
            {
                if (request.Request.Method == HttpMethod.Get)
                {
                    var etag = OutputCacheHandler.ETag(request.CurrentCacheKey);
                    if (etag != null && request.Request.Headers.IfNoneMatch.Contains(etag))
                {
                    return new HttpResponseMessageWrapper(
                        request.Request.CreateResponse(HttpStatusCode.NotModified),
                        request.Request.RequestUri,
                        DecalringType,
                        _varyByParam);
                }
                }
                return null;
            };
        }

        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> InvalidateETag()
        {
            return delegate(HttpResponseMessageWrapper response)
            {
                OutputCacheHandler.InvalidateETag(response.CurrentCacheKey);
                return response;
            };
        }


        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> InvalidateOutputCache()
        {
            return delegate(HttpRequestMessageWrapper request)
            {
                OutputCacheHandler.InvalidateOutputCache(request.CurrentCacheKey);
                return null;
            };
        }



        private Func<HttpResponseMessageWrapper, HttpResponseMessageWrapper> SetPublicCacheHeaders()
        {
            return delegate(HttpResponseMessageWrapper response)
            {
                CacheControlHeaderValue value2 = new CacheControlHeaderValue
                {
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
            return delegate(HttpResponseMessageWrapper response)
            {
                SynchronizedCacheManager.Instance.Set(response.CurrentCacheKey, response);
                return response;
            };
        }

        private Func<HttpRequestMessageWrapper, HttpResponseMessageWrapper> TryDeliverFromOutputCache()
        {
            return request => OutputCacheHandler.Get(request.CurrentCacheKey);
        }

        public bool CacheOnServer { get; set; }

        public Type DecalringType { get; set; }



        public virtual Type Key { get; set; }


        public virtual bool Update { get; set; }

        public bool VaryByUser { get; set; }

        private List<String> _varyByParam;
        public string VarByParam
        {
            set
            {
                _varyByParam = new List<string>(value.Split(new[] { ',' }));
            }
        }

        public bool VarByPath { get; set; }

        public bool VaryByPath { get; set; }
    }
}

