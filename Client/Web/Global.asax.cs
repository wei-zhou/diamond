namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.SessionState;
    using Home.Forms;

    public class Global : System.Web.HttpApplication
    {
        private readonly IReadOnlyDictionary<string, Tuple<string, string>> webFormsRoutes = new Dictionary<string, Tuple<string, string>>()
        {
        };

        protected void Application_Error()
        {
            HandleWebFormsError();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
        }

        private void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // MVC routes

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                name: "Pages",
                url: "pages/{id}",
                defaults: new { controller = "Home", action = "Page" }
            );

            routes.MapRoute(
                name: "Permissions",
                url: "permissions",
                defaults: new { controller = "Home", action = "Permissions" }
            );

            routes.MapRoute(
                name: "Login",
                url: "account/login",
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "Logout",
                url: "account/logout",
                defaults: new { controller = "Account", action = "Logout" }
            );

            routes.MapRoute(
                name: "CheckLogin",
                url: "account/check",
                defaults: new { controller = "Account", action = "CheckLogin" }
            );

            routes.MapRoute(
                name: "ProductDiamondFileUpload",
                url: "product/diamond/upload",
                defaults: new { controller = "Product", action = "UploadDiamondFile" }
            );

            // Web forms routes

            foreach (var route in webFormsRoutes)
            {
                routes.MapPageRoute(route.Key, route.Value.Item1, route.Value.Item2, true, null, null, null);
            }
        }

        private void HandleWebFormsError()
        {
            var isWebFormsRequest = false;
            foreach (var item in this.webFormsRoutes.Values)
            {
                var segments = item.Item1.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.AreEqual(this.Request.Url.Segments.Skip(1).Select(s => s.Trim('/'))))
                {
                    isWebFormsRequest = true;
                    break;
                }
            }

            if (isWebFormsRequest)
            {
                var exception = this.Server.GetLastError();
                if (exception != null)
                {
                    WebFormsErrorHelper.Handle(exception, this.Request, this.Response, this.User);
                }
            }
        }
    }
}