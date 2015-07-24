namespace Home.Services.SecurityService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.OData;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;
    using Owin;

    public static class SecurityServiceHelper
    {
        public static class RoleConstants
        {
            public const string Employee = "Employee";
            public const string Manager = "Manager";
            public const string Administrator = "Administrator";
        }

        public static void ConfigureSecurityService(this IAppBuilder app)
        {
            Debug.Assert(app != null);

            //// build EDM model

            var builder = new ODataConventionModelBuilder()
            {
                Namespace = typeof(UsersController).Namespace,
                ContainerName = "DefaultContainer"
            };

            builder.EnumType<Role>();
            // TODO: how to mark the entity set as readonly
            var userEntityType = builder.EntitySet<User>("Users").EntityType;
            userEntityType.Ignore(c => c._Password);
            userEntityType.Ignore(c => c._IsLocked);
            userEntityType.Ignore(c => c._LockDateTimeUtc);

            var edmModel = builder.GetEdmModel();

            //// configure web API

            var config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            config.MessageHandlers.Add(new ETagMessageHandler());
            config.MapODataServiceRoute("SecurityService", "securityservice", edmModel);
            config.Services.Replace(typeof(IHttpControllerSelector), new ClientHttpControllerSelector(config, typeof(SecurityServiceHelper)));
            config.EnsureInitialized();

            app.UseWebApi(config);
        }
    }
}
