namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Configuration;

    public static class ConfigurationHelper
    {
        public static bool IsDevelopmentEnvironment
        {
            get { return WebConfigurationManager.AppSettings["home:Environment"] == "Development"; }
        }

        public static bool IsUnitTestEnvironment
        {
            get { return WebConfigurationManager.AppSettings["home:Environment"] == "UnitTest"; }
        }

        public static bool IsE2ETestEnvironment
        {
            get { return WebConfigurationManager.AppSettings["home:Environment"] == "E2ETest"; }
        }

        public static bool IsProductionEnvironment
        {
            get { return WebConfigurationManager.AppSettings["home:Environment"] == "Production"; }
        }

        public static string ProductVersion
        {
            get { return WebConfigurationManager.AppSettings["home:ProductVersion"]; }
        }

        public static TimeSpan SessionTimeoutInterval
        {
            get { return TimeSpan.FromMinutes(double.Parse(WebConfigurationManager.AppSettings["home:SessionTimeoutInterval"], NumberStyles.Number, CultureInfo.InvariantCulture)); }
        }
    }
}