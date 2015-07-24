namespace Home.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Web;
    using System.Web.Routing;

    public static class WebFormsErrorHelper
    {
        public static void Handle(Exception exception, HttpRequest request, HttpResponse response, IPrincipal user)
        {
            Debug.Assert(exception != null);
            Debug.Assert(request != null);
            Debug.Assert(response != null);
            Debug.Assert(user != null);

            var message = new StringBuilder();
            Format(message, request, "HttpRequest");
            Format(message, response, "HttpResponse");
            HandleErrorHelper.Format(message, exception, "Exception");
            HandleErrorHelper.Format(message, user, "IPrincipal");
            Trace.TraceError(message.ToString());
        }

        private static void Format(StringBuilder message, HttpRequest request, string header)
        {
            HandleErrorHelper.AppendHeader(message, header);

            if (request == null)
            {
                message.AppendLine(HandleErrorHelper.NULL);
                message.AppendLine();
                return;
            }

            message.AppendFormat("Http method: {0}", request.HttpMethod);
            message.AppendLine();
            message.AppendFormat("URL: {0}", request.Url.ToString());
            message.AppendLine();
            HandleErrorHelper.Format(message, request.Headers, "HTTP Headers", false);
            message.AppendLine();
        }

        private static void Format(StringBuilder message, HttpResponse response, string header)
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
    }
}