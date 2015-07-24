namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public abstract class Product<TImport, TProduct> : Entity
        where TImport : ProductImport<TImport, TProduct>
        where TProduct : Product<TImport, TProduct>
    {
        public int Index { get; set; }

        public ProductStatus Status { get; set; }

        public decimal Cost { get; set; }

        public decimal SalePrice { get; set; }

        public string Vendor { get; set; }

        public Guid _ImportId { get; set; }

        public TImport _Import { get; set; }
    }
}