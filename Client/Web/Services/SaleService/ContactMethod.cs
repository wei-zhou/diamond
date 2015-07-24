namespace Home.Services.SaleService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public enum ContactMethod : byte
    {
        QQ = 1,
        Phone,
        Email,
        WeiXin,
        Other
    }
}
