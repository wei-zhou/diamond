namespace Home.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    public static class MvcHandleErrorHelper
    {
        public static void Handle(ExceptionContext context)
        {
            Debug.Assert(context != null);

            var message = new StringBuilder();
            Format(message, context.HttpContext.Request, "ExceptionContext.HttpContextBase.HttpRequestBase");
            Format(message, context.HttpContext.Response, "ExceptionContext.HttpContextBase.HttpResponseBase");
            HandleErrorHelper.Format(message, context.Exception, "Exception");
            Format(message, context.RouteData, "ExceptionContext.RouteData");
            HandleErrorHelper.Format(message, context.HttpContext.User, "ExceptionContext.HttpContextBase.User");
            Trace.TraceError(message.ToString());
        }

        private static void Format(StringBuilder message, HttpRequestBase request, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);

            if (request == null)
            {
                message.AppendLine(HandleErrorHelper.NULL);
                message.AppendLine();
                return;
            }

            message.AppendFormat("IsAjaxRequest: {0}", request.IsAjaxRequest());
            message.AppendLine();
            message.AppendFormat("Http method: {0}", request.HttpMethod);
            message.AppendLine();
            message.AppendFormat("URL: {0}", request.Url.ToString());
            message.AppendLine();
            HandleErrorHelper.Format(message, request.Headers, "HTTP Headers", false);
            message.AppendLine();
        }

        private static void Format(StringBuilder message, HttpResponseBase response, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);

            if (response == null)
            {
                message.AppendLine(HandleErrorHelper.NULL);
                message.AppendLine();
                return;
            }

            message.AppendFormat("Status: {0}", response.Status);
            message.AppendLine();
            message.AppendFormat("StatusCode: {0}", response.StatusCode);
            message.AppendLine();
            message.AppendFormat("StatusDescription: {0}", response.StatusDescription);
            message.AppendLine();
            message.AppendFormat("SubStatusCode: {0}", response.SubStatusCode);
            message.AppendLine();
            HandleErrorHelper.Format(message, response.Headers, "HTTP Headers", false);
            message.AppendLine();
        }

        private static void Format(StringBuilder message, RouteData route, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);

            if (route == null)
            {
                message.AppendLine(HandleErrorHelper.NULL);
                message.AppendLine();
                return;
            }

            var controllerName = (string)route.Values["controller"];
            var actionName = (string)route.Values["action"];

            message.AppendFormat("Controller: {0}", controllerName);
            message.AppendLine();
            message.AppendFormat("Action: {0}", actionName);
            message.AppendLine();
            message.AppendLine();
        }
    }
}