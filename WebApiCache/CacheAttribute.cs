using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApiCache.Rules;
using WebApiCache.Rules.RequestRules;
using WebApiCache.Rules.ResponseRules;

namespace WebApiCache
{
    public class CacheAttribute : ActionFilterAttribute
    {
        private List<IRequestRule> _requestRules;
        private List<IResponseRule> _responseRules;
        private bool _isInitialized;
        private object _initializeLock = new object();
        
        public CacheAttribute()
        {
            _requestRules = new List<IRequestRule>();
            _responseRules = new List<IResponseRule>();
            PreInitialize();
        }

        /// <summary>
        /// Initialize this attribute when the object is created, before the properties are added..
        /// </summary>
        private void PreInitialize()
        {
            _requestRules.Add(new StoreCacheKeyOnInternalRequestObject(this));
            _responseRules.Add(new StoreCacheKeyonInternalResponseObject(this));
        }

        /// <summary>
        /// Initialize the attribute with a action context, this happens on the first request accessing the decorated action methos.
        /// </summary>
        /// <param name="actionContext">The action context of the decorated action.</param>
        private void OnFirstRequestInitialize(HttpActionContext actionContext)
        {
            if (DecalringType == null)
            {
                DecalringType = actionContext.ControllerContext.ControllerDescriptor.ControllerType;
            }

            if (!Update)
            {
                _requestRules.Add(new IfNoneMatch());
                if (CacheOnServer)
                {
                    _requestRules.Add(new TryDeliverFromOutputCache());
                }
            }
            else
            //Updates
            {
                if (CacheOnServer)
                {
                    _requestRules.Add(new InvalidateOutputCache());
                }
                _responseRules.Add(new InvalidateETag());
            }

            _responseRules.Add(new ApplyETag());
            if (CacheOnServer)
            {
                _responseRules.Add(new StoreInOutputCache());
            }
            _responseRules.Add(new SetPublicCacheHeaders());
        }


        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.Response = null;
            if (!_isInitialized)
            {
                lock (_initializeLock)
                {
                    if (!_isInitialized)
                    {
                        _isInitialized = true;
                        OnFirstRequestInitialize(actionContext);
                    }
                }
            }

            var request = new HttpRequestMessageWrapper(actionContext.Request);

            foreach (IRequestRule rule in _requestRules)
            {
                HttpResponseMessageWrapper message = rule.Invoke(request);
                if (message != null)
                {
                    actionContext.Response = message.Response;
                }
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var response = new HttpResponseMessageWrapper(actionExecutedContext.Response, actionExecutedContext.Request.RequestUri);

            foreach (IResponseRule func in _responseRules)
            {
                func.Invoke(response);
            }

            base.OnActionExecuted(actionExecutedContext);
        }

        
        public bool CacheOnServer { get; set; }

        public Type DecalringType { get; set; }

        public virtual Type Key { get; set; }

        public virtual bool Update { get; set; }

        public bool VaryByUser
        {
            set
            {
                if (!value) return;

                _requestRules.Add(new AppendUserCacheKey());
                _responseRules.Add(new AppendUserCacheKeyForResponse());
            }
        }

        readonly char[] _paramSplitter = new[] { ',' };
        public string VarByParam
        {
            set
            {
                _varyByParams = value.Split(_paramSplitter);
                if (_varyByParams.Length == 0) return;

                _requestRules.Add(new AppendParamCacheKey(this));
                _responseRules.Add(new AppendParamCacheKeyForResponse(this));
            }
        }

        public bool VaryByPath
        {
            set
            {
                if (!value) return;

                _requestRules.Add(new AppendPathCacheKey());
                _responseRules.Add(new AppendPathCacheKeyForResponse());
            }
        }

        private string[] _varyByParams;
        public string[] GetVarByParams()
        {
            return _varyByParams;
        }
    }
}

