namespace Home.Services
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using System.Web.Http.ModelBinding;

    public static class ServiceHandleErrorHelper
    {
        public const string SalesServiceName = "Sales Service";
        public const string SecurityServiceName = "Security Service";
        public const string SsisServiceName = "SSIS Service";
        public const string ProductServiceName = "Product Service";

        public static void Handle(HttpActionExecutedContext context, string header)
        {
            Debug.Assert(context != null);
            Debug.Assert(!string.IsNullOrEmpty(header));

            var message = new StringBuilder();
            message.AppendLine(header);
            Format(message, context.Request, "HttpRequestMessage");
            Format(message, context.Response, "HttpResponseMessage");
            HandleErrorHelper.Format(message, context.Exception, "Exception");
            Format(message, context.ActionContext, "HttpActionContext");
            Trace.TraceError(message.ToString());
        }

        private static void Format(StringBuilder message, HttpRequestMessage request, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);
            message.AppendLine(request == null ? HandleErrorHelper.NULL : request.ToString());
            message.AppendLine();
        }

        private static void Format(StringBuilder message, HttpResponseMessage response, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);
            message.AppendLine(response == null ? HandleErrorHelper.NULL : response.ToString());
            message.AppendLine();
        }

        private static void Format(StringBuilder message, HttpActionContext context, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);

            if (context == null)
            {
                message.AppendLine(HandleErrorHelper.NULL);
                message.AppendLine();
                return;
            }

            Format(message, context.ActionDescriptor, "HttpActionContext.ActionDescriptor");
            HandleErrorHelper.Format(message, context.ActionArguments, "HttpActionContext.ActionArguments");
            Format(message, context.ModelState, "HttpActionContext.ModelState");
            Format(message, context.RequestContext, "HttpActionContext.HttpRequestContext");
        }

        private static void Format(StringBuilder message, HttpActionDescriptor descriptor, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);

            if (descriptor == null)
            {
                message.AppendLine(HandleErrorHelper.NULL);
                message.AppendLine();
                return;
            }

            message.AppendFormat("Controller name: {0}", descriptor.ControllerDescriptor.ControllerName);
            message.AppendLine();
            message.AppendFormat("Controller type: {0}", descriptor.ControllerDescriptor.ControllerType.FullName);
            message.AppendLine();
            message.AppendFormat("Action name: {0}", descriptor.ActionName);
            message.AppendLine();
            message.AppendLine();
        }

        private static void Format(StringBuilder message, ModelStateDictionary state, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);

            if (state == null)
            {
                message.AppendLine(HandleErrorHelper.NULL);
                message.AppendLine();
                return;
            }

            message.AppendFormat("IsValid: {0}", state.IsValid);
            message.AppendLine();

            foreach (var item in state)
            {
                message.AppendFormat("Key: {0}", item.Key);
                message.AppendLine();
                foreach (var error in item.Value.Errors)
                {
                    message.AppendFormat("Error message: {0}", error.ErrorMessage == null ? HandleErrorHelper.NULL : error.ErrorMessage);
                    message.AppendLine();
                    message.AppendFormat("Error exception: {0}", error.Exception == null ? HandleErrorHelper.NULL : error.Exception.ToString());
                    message.AppendLine();
                }
            }

            message.AppendLine();
        }

        private static void Format(StringBuilder message, HttpRequestContext context, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);

            if (context == null)
            {
                message.AppendLine(HandleErrorHelper.NULL);
                message.AppendLine();
                return;
            }

            HandleErrorHelper.Format(message, context.Principal, "HttpActionContext.HttpRequestContext.Principal");
            HandleErrorHelper.Format(message, context.ClientCertificate, "HttpActionContext.HttpRequestContext.ClientCertificate");
        }
    }
}