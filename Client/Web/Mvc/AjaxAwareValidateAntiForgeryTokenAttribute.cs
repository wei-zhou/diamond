namespace Home.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AjaxAwareValidateAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            if (request.IsAjaxRequest())
            {
                string cookieToken = "";
                string formToken = "";

                var tokenHeaders = request.Headers.GetValues(CommonHelper.AntiForgeryAjaxHeaderName);
                if (tokenHeaders.Length > 0)
                {
                    var tokens = tokenHeaders.First().Split(':');
                    if (tokens.Length == 2)
                    {
                        cookieToken = tokens[0].Trim();
                        formToken = tokens[1].Trim();
                    }
                }

                AntiForgery.Validate(cookieToken, formToken);
            }
            else
            {
                AntiForgery.Validate();
            }
        }
    }
}