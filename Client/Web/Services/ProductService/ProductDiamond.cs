namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class ProductDiamond : Product<ProductDiamondImport, ProductDiamond>
    {
        public ProductDiamondReport? ReportType { get; set; }

        public string ReportNumber { get; set; }

        public decimal? Caret { get; set; }

        public ProductDiamondClarity? Clarity { get; set; }

        public ProductDiamondColor? Color { get; set; }

        public ProductDiamondCut? Cut { get; set; }

        public ProductDiamondPolish? Polish { get; set; }

        public ProductDiamondSymmetry? Symmetry { get; set; }

        public string Fluorescence { get; set; }

        public string Comment { get; set; }
    }
}