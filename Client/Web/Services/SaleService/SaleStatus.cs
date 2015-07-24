namespace Home.Services.SaleService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public enum SaleStatus : byte
    {
        Ok = 1,
        Bad,
        Returned
    }
}
