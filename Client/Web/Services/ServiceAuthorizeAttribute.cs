namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Home.Services.SecurityService;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ServiceAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (ConfigurationHelper.IsE2ETestEnvironment)
            {
                return true;
            }

            var user = actionContext.Request.GetOwinContext().Authentication.User;
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }

            if (user.IsInRole(SecurityServiceHelper.RoleConstants.Administrator))
            {
                return true;
            }

            return base.IsAuthorized(actionContext);
        }
    }
}