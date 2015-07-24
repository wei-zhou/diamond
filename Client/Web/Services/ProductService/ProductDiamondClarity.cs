namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Serializable]
    public enum ProductDiamondClarity : byte
    {
        FL = 1,
        IF,
        VVS1,
        VVS2,
        VS1,
        VS2,
        SI1,
        SI2,
        I1,
        I2,
        I3
    }
}