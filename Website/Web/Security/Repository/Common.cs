namespace Home.Security.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class Common
    {
        public static readonly Guid UserWeiZhouId = new Guid("{797C7DBE-99F1-4A98-ABA4-5139F364A76D}");
        public static readonly Guid UserLvLeiYanId = new Guid("{4D868101-F0BC-4E68-B62E-B08C9DA01BC2}");
        public static readonly Guid UserJiYanQinId = new Guid("{3241DFC0-CAB2-41EE-8EA9-052C552D0B0A}");

        public const byte RoleUnknown = 0;
        public const byte RoleSuperAdministrator = 1;
        public const byte RoleAdministrator = 2;
        public const byte RoleVisitor = 3;

        public const byte IPUnknown = 0;
        public const byte IPQQ = 1;
        public const byte IPSina = 2;
        public const byte IPWeiXin = 3;
    }
}
