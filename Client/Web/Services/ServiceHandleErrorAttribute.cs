namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Web;
    using System.Web.Http.Filters;

    public class ServiceHandleErrorAttribute : ExceptionFilterAttribute
    {
        public string ServiceName { get; set; }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Debug.Assert(actionExecutedContext != null);

            ServiceHandleErrorHelper.Handle(actionExecutedContext, this.ServiceName);
        }
    }
}