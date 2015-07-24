namespace Home.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Home.Services.SecurityService;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class MvcAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (ConfigurationHelper.IsE2ETestEnvironment)
            {
                return true;
            }

            var user = httpContext.GetOwinContext().Authentication.User;
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }

            if (user.IsInRole(SecurityServiceHelper.RoleConstants.Administrator))
            {
                return true;
            }

            return base.AuthorizeCore(httpContext);
        }
    }
}