using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApiCache;

namespace CacheWebApiTests
{
    [DebuggerDisplay("ETag = {ETag}")]
    public class Browser
    {
        private HttpActionExecutedContext actionExecutedContext;
        private HttpActionContext actionExecutingContext;

        public Browser()
        {
            HttpActionContext actionContext = new HttpActionContext();
            HttpControllerContext controllerContext = new HttpControllerContext();
            HttpControllerDescriptor descriptor = new HttpControllerDescriptor
            {
                ControllerType = typeof(TestController)
            };
            controllerContext.ControllerDescriptor = descriptor;
            controllerContext.Request = new HttpRequestMessage();
            actionContext.ControllerContext = controllerContext;
            actionContext.Response = new HttpResponseMessage();
            actionExecutingContext = actionContext;
            actionExecutedContext = new HttpActionExecutedContext()
            {
                ActionContext = actionContext,
                Response = new HttpResponseMessage()
            };
        }

        public Browser(EntityTagHeaderValue entityTagHeaderValue)
            : this()
        {
            actionExecutingContext.Request.Headers.IfNoneMatch.Add(entityTagHeaderValue);
        }

        public Browser(Uri uri)
            : this()
        {
            actionExecutingContext.Request.RequestUri = uri;
        }

        public void ExecuteControllerAction(WebApiCacheAttribute filter)
        {
            _controllerExecuted = true;
            actionExecutingContext.Response = new HttpResponseMessage();
            filter.OnActionExecuted(actionExecutedContext);
        }

        public void MakeRequest(WebApiCacheAttribute filter)
        {
            filter.OnActionExecuting(actionExecutingContext);
            if (actionExecutingContext.Response == null)
            {
                ExecuteControllerAction(filter);
            }
        }

        public HttpActionExecutedContext ActionExecutedContext
        {
            get
            {
                return actionExecutedContext;
            }
        }

        public HttpActionContext ActionExecutingContext
        {
            get
            {
                return actionExecutingContext;
            }
        }

        bool _controllerExecuted;
        public bool ControllerExecuted
        {
            get
            {
                return _controllerExecuted;
            }
        }

        public Type ControllerType
        {
            get
            {
                return actionExecutingContext.ControllerContext.ControllerDescriptor.ControllerType;
            }
            set
            {
                actionExecutingContext.ControllerContext.ControllerDescriptor.ControllerType = value;
            }
        }

        public EntityTagHeaderValue ETag
        {
            get
            {
                return actionExecutingContext.Response != null
                    ? actionExecutingContext.Response.Headers.ETag
                    : actionExecutedContext.Response.Headers.ETag;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return actionExecutingContext.Response != null
                    ? actionExecutingContext.Response.StatusCode
                    : actionExecutedContext.Response.StatusCode;
            }
        }
    }
}

