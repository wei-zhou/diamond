namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public abstract class ProductImport<TImport, TProduct> : Entity
        where TImport : ProductImport<TImport, TProduct>
        where TProduct : Product<TImport, TProduct>
    {
        /// <summary>
        /// The total row count that imported.
        /// </summary>
        public int Count { get; set; }

        public virtual IList<TProduct> Products { get; set; }
    }
}