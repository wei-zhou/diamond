namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Serializable]
    public enum ProductDiamondReport : byte
    {
        GIA = 1,
        IGI,
        EGL,
        China
    }
}