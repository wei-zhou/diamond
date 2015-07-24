namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.Text;

    public static class HandleErrorHelper
    {
        public const string NULL = "<null>";

        public static void Handle(Exception exception, params object[] args)
        {
            Debug.Assert(exception != null);

            var message = new StringBuilder();
            Format(message, args, "Information");
            Format(message, exception, "Exception");
            Trace.TraceError(message.ToString());
        }

        public static void Format(StringBuilder message, IDictionary<string, object> dictionary, string header)
        {
            Debug.Assert(message != null);
            Debug.Assert(!string.IsNullOrEmpty(header));

            AppendHeader(message, header);

            if (dictionary == null)
            {
                message.AppendLine(NULL);
                message.AppendLine();
                return;
            }

            foreach (var item in dictionary)
            {
                message.AppendFormat("{0}: {1}", item.Key, item.Value == null ? NULL : item.Value.ToString());
                message.AppendLine();
            }

            message.AppendLine();
        }

        public static void Format(StringBuilder message, NameValueCollection collection, string header, bool separate)
        {
            Debug.Assert(message != null);
            Debug.Assert(!string.IsNullOrEmpty(header));

            AppendHeader(message, header);

            if (collection == null)
            {
                message.AppendLine(NULL);
                message.AppendLine();
                return;
            }

            foreach (string key in collection.Keys)
            {
                var value = collection[key];
                message.AppendFormat("{0}: {1}", key, value == null ? NULL : value);
                message.AppendLine();
            }

            if (separate)
            {
                message.AppendLine();
            }
        }

        public static void Format(StringBuilder message, IPrincipal principal, string header)
        {
            Debug.Assert(message != null);
            Debug.Assert(!string.IsNullOrEmpty(header));

            AppendHeader(message, header);

            if (principal == null)
            {
                message.AppendLine(NULL);
                message.AppendLine();
                return;
            }

            message.AppendFormat("Principal type: {0}", principal.GetType().Name);
            message.AppendLine();
            message.AppendFormat("Principal.Identity type: {0}", principal.Identity.GetType().Name);
            message.AppendLine();
            message.AppendFormat("Principal.ToString(): {0}", principal.ToString());
            message.AppendLine();
            message.AppendFormat("Principal.Identity.ToString(): {0}", principal.Identity.ToString());
            message.AppendLine();
            message.AppendFormat("Principal.Identity.Name: {0}", principal.Identity.Name);
            message.AppendLine();
            message.AppendFormat("Principal.Identity.IsAuthenticated: {0}", principal.Identity.IsAuthenticated);
            message.AppendLine();
            message.AppendFormat("Principal.Identity.AuthenticationType: {0}", principal.Identity.AuthenticationType);
            message.AppendLine();
            message.AppendLine();
        }

        public static void Format(StringBuilder message, X509Certificate2 certificate, string header)
        {
            Debug.Assert(message != null);
            Debug.Assert(!string.IsNullOrEmpty(header));

            AppendHeader(message, header);

            if (certificate == null)
            {
                message.AppendLine(NULL);
                message.AppendLine();
                return;
            }

            message.AppendFormat("X509Certificate2.ToString(): {0}", certificate.ToString());
            message.AppendLine();
            message.AppendLine();
        }

        public static void Format(StringBuilder message, Exception exception, string header)
        {
            Debug.Assert(message != null);
            Debug.Assert(!string.IsNullOrEmpty(header));

            AppendHeader(message, header);

            do
            {
                message.AppendFormat("Exception.ToString(){0}{1}{0}", Environment.NewLine, exception);
                message.AppendFormat("Exception.Message{0}{1}{0}", Environment.NewLine, exception.Message);
                message.AppendFormat("Exception.StackTrace{0}{1}{0}", Environment.NewLine, exception.StackTrace);

                exception = exception.InnerException;
                if (exception != null)
                {
                    message.AppendLine(new string('*', 50));
                    continue;
                }
            } while (false);

            message.AppendLine();
        }

        public static void Format(StringBuilder message, object[] args, string header)
        {
            Debug.Assert(message != null);
            Debug.Assert(!string.IsNullOrEmpty(header));

            AppendHeader(message, header);

            if (args != null)
            {
                foreach (var item in args)
                {
                    if (item == null)
                    {
                        message.AppendLine(NULL);
                    }
                    else
                    {
                        message.AppendLine(item.ToString());
                    }
                }
            }

            message.AppendLine();
        }

        public static void AppendHeader(StringBuilder message, string header)
        {
            Debug.Assert(message != null);
            Debug.Assert(!string.IsNullOrEmpty(header));

            message.AppendFormat("{1}{2}{1}{0}", Environment.NewLine, new string('-', 10), header);
        }
    }
}
