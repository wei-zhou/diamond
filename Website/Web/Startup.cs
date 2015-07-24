namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
        }
    }
}