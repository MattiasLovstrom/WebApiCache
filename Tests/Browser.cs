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
            HttpActionContext context = new HttpActionContext();
            HttpControllerContext context2 = new HttpControllerContext();
            HttpControllerDescriptor descriptor = new HttpControllerDescriptor {
                ControllerType = typeof(TestController)
            };
            context2.ControllerDescriptor = descriptor;
            context2.Request = new HttpRequestMessage();
            context.ControllerContext = context2;
            context.Response = new HttpResponseMessage();
            this.actionExecutingContext = context;
            HttpActionExecutedContext context3 = new HttpActionExecutedContext {
                ActionContext = new HttpActionContext(),
                Response = new HttpResponseMessage()
            };
            this.actionExecutedContext = context3;
        }

        public Browser(EntityTagHeaderValue entityTagHeaderValue) : this()
        {
            this.actionExecutingContext.Request.Headers.IfNoneMatch.Add(entityTagHeaderValue);
        }

        public void MakeRequest(WebApiCacheAttribute filter)
        {
            filter.OnActionExecuting(this.actionExecutingContext);
            if (this.actionExecutingContext.Response == null)
            {
                filter.OnActionExecuted(this.actionExecutedContext);
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

        public bool ControllerExecuted
        {
            get
            {
                return (actionExecutingContext.Response == null);
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

