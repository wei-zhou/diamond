namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Serializable]
    public enum ProductDiamondCut : byte
    {
        EX = 1,
        VG,
        G,
        F,
        P
    }
}