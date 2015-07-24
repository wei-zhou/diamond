[assembly: Microsoft.Owin.OwinStartup(typeof(Home.Startup))]

namespace Home
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Home.Services.ProductService;
    using Home.Services.SaleService;
    using Home.Services.SecurityService;
    using Home.Services.SsisService;
    using Microsoft.Owin;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //// security

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CommonHelper.DefaultAuthenticationType,
                ExpireTimeSpan = ConfigurationHelper.SessionTimeoutInterval,
                LoginPath = new PathString("/account/login"),
                LogoutPath = new PathString("/account/logout"),
                ReturnUrlParameter = "return_url",
                Provider = new CookieAuthenticationProvider
                {
                    OnApplyRedirect = context =>
                    {
                        if (!context.Request.IsAjaxRequest())
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                    },
                }
            });

            //// odata service

            app.ConfigureProductService();
            app.ConfigureSalesService();
            app.ConfigureSecurityService();
            app.ConfigureSsisService();
        }
    }
}
