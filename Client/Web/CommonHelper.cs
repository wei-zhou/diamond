namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using Microsoft.Owin;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class CommonHelper
    {
        public const string DefaultAuthenticationType = "ApplicationCookie";
        public const string IdentityProviderClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
        public const string IdentityProvider = "Home Client Identity Provider";
        public const string AntiForgeryAjaxFieldName = "__AjaxRequestVerificationToken";
        public const string AntiForgeryAjaxHeaderName = "RequestVerificationToken";

        public static bool AreEqual<T>(this IEnumerable<T> left, IEnumerable<T> right)
        {
            Debug.Assert(left != null);
            Debug.Assert(right != null);

            if (left.Count() != right.Count())
            {
                return false;
            }

            for (int i = 0; i < left.Count(); i++)
            {
                if (!object.Equals(left.ElementAt(i), right.ElementAt(i)))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsAjaxRequest(this IOwinRequest request)
        {
            Debug.Assert(request != null);

            var query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }

            var headers = request.Headers;
            return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
        }

        public static JToken GetJsonBody(this IOwinRequest request)
        {
            Debug.Assert(request != null);

            using (var streamReader = new StreamReader(request.Body, Encoding.UTF8, true))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    return JToken.Load(jsonReader);
                }
            }
        }

        public static string GetAntiForgeryValue()
        {
            string cookieToken, formToken;
            AntiForgery.GetTokens(null, out cookieToken, out formToken);
            return cookieToken + ":" + formToken;
        }

        public static MvcHtmlString AjaxAntiForgeryToken(this HtmlHelper helper)
        {
            Debug.Assert(helper != null);

            var builder = new TagBuilder("input");
            builder.Attributes["type"] = "hidden";
            builder.Attributes["name"] = AntiForgeryAjaxFieldName;
            builder.Attributes["value"] = GetAntiForgeryValue();
            return new MvcHtmlString(builder.ToString(TagRenderMode.SelfClosing));
        }
    }
}