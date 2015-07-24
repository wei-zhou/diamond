namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Serializable]
    public enum ProductStatus : byte
    {
        Active = 1,
        Inactive,
        Sold
    }
}