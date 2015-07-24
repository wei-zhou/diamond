namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.SessionState;
    using Home.Security.Web;

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(RegisterWebApiRoutes);
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                name: Common.Route.Login.Name,
                url: Common.Route.Login.Url,
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: Common.Route.LoginCallback.Name,
                url: Common.Route.LoginCallback.Url,
                defaults: new { controller = "Account", action = "LoginCallback" }
            );

            routes.MapRoute(
                name: Common.Route.LoginFailure.Name,
                url: Common.Route.LoginFailure.Url,
                defaults: new { controller = "Account", action = "LoginFailure" }
            );

            routes.MapRoute(
                name: Common.Route.LogOff.Name,
                url: Common.Route.LogOff.Url,
                defaults: new { controller = "Account", action = "LogOff" }
            );
        }

        private static void RegisterWebApiRoutes(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}