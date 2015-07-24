namespace Home.Security.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class Common
    {
        public static class Route
        {
            public static class Login
            {
                public const string Name = "Login";
                public const string Url = "login";
            }

            public static class LoginCallback
            {
                public const string Name = "LoginCallback";
                public const string Url = "login-callback";
            }

            public static class LoginFailure
            {
                public const string Name = "LoginFailure";
                public const string Url = "login-failure";
            }

            public static class LogOff
            {
                public const string Name = "LogOff";
                public const string Url = "logoff";
            }
        }

        public static class IdentityProvider
        {
            public static class Name
            {
                public const string QQ = "qq";
                public const string Sina = "sina";
                public const string WeiXin = "weixin";
            }

            internal static class Location
            {
#if DEBUG
                public const string QQ = "http://localhost:23456/qq/";
                public const string Sina = "http://localhost:23456/sina/";
                public const string WeiXin = "http://localhost:23456/weixin/authorize";
#else
#endif
            }
        }

        internal static class View
        {
            public static class VirtualPath
            {
                internal const string Login = "~/Views/Account/Login.cshtml";
                internal const string LoginFailure = "~/Views/Account/LoginFailure.cshtml";
                internal const string LoginLockout = "~/Views/Account/LoginLockout.cshtml";
            }
        }
    }
}
