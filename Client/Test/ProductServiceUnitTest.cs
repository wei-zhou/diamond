namespace Home.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Home.Mvc.Models;
    using Home.Services.ProductService;
    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProductServiceUnitTest : BaseTest
    {
        #region Initialize & cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            ProductServiceTestHelper.CleanupDatabase();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ProductServiceTestHelper.CleanupDatabase();
        }

        #endregion

        #region Upload diamond

        [TestMethod]
        [TestCategory("UnitTest")]
        public void UploadDiamond_Append()
        {
            ProductServiceTestHelper.UploadDiamond("product_diamond_1.csv", ProductFileUploadType.Replace);
            ProductServiceTestHelper.UploadDiamond("product_diamond_3.csv", ProductFileUploadType.Append);
            var imports = default(IEnumerable<ProductDiamondImport>);
            var diamonds = default(IEnumerable<ProductDiamond>);
            using (var db = new ProductDbContext())
            {
                imports = db.DiamondImports.OrderBy(d => d.Created).ToArray();
                diamonds = db.Diamonds.ToArray();
            }

            Assert.AreEqual(2, imports.Count());
            Assert.AreEqual(3, diamonds.Count());

            {
                var import = imports.ElementAt(0);
                Assert.AreEqual(2, import.Count);
            }

            {
                var import = imports.ElementAt(1);
                Assert.AreEqual(1, import.Count);
            }

            ValidateDiamond(diamonds, 1, false);
            ValidateDiamond(diamonds, 2, false);
            ValidateDiamond(diamonds, 3, false);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void UploadDiamond_Replace()
        {
            ProductServiceTestHelper.UploadDiamond("product_diamond_1.csv", ProductFileUploadType.Replace);
            ProductServiceTestHelper.UploadDiamond("product_diamond_3.csv", ProductFileUploadType.Replace);
            var imports = default(IEnumerable<ProductDiamondImport>);
            var diamonds = default(IEnumerable<ProductDiamond>);
            using (var db = new ProductDbContext())
            {
                imports = db.DiamondImports.OrderBy(d => d.Created).ToArray();
                diamonds = db.Diamonds.ToArray();
            }

            Assert.AreEqual(2, imports.Count());
            Assert.AreEqual(1, diamonds.Count());

            {
                var import = imports.ElementAt(0);
                Assert.AreEqual(2, import.Count);
            }

            {
                var import = imports.ElementAt(1);
                Assert.AreEqual(1, import.Count);
            }

            ValidateDiamond(diamonds, 3, false);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void UploadDiamond_Update()
        {
            ProductServiceTestHelper.UploadDiamond("product_diamond_1.csv", ProductFileUploadType.Replace);
            ProductServiceTestHelper.UploadDiamond("product_diamond_2.csv", ProductFileUploadType.Update);
            var imports = default(IEnumerable<ProductDiamondImport>);
            var diamonds = default(IEnumerable<ProductDiamond>);
            using (var db = new ProductDbContext())
            {
                imports = db.DiamondImports.OrderBy(d => d.Created).ToArray();
                diamonds = db.Diamonds.ToArray();
            }

            Assert.AreEqual(2, imports.Count());
            Assert.AreEqual(2, diamonds.Count());

            {
                var import = imports.ElementAt(0);
                Assert.AreEqual(2, import.Count);
            }

            {
                var import = imports.ElementAt(1);
                Assert.AreEqual(1, import.Count);
            }

            ValidateDiamond(diamonds, 1, false);
            ValidateDiamond(diamonds, 2, true);
        }

        #endregion

        #region Helpers

        private static void ValidateDiamond(IEnumerable<ProductDiamond> diamonds, int index, bool updated)
        {
            switch (index)
            {
                case 1:
                    {
                        var diamond = diamonds.Single(d => d.Index == 1);
                        Assert.AreEqual(10000, diamond.Cost);
                        Assert.AreEqual(20000, diamond.SalePrice);
                        Assert.AreEqual(null, diamond.Vendor);
                        Assert.AreEqual(ProductDiamondReport.GIA, diamond.ReportType);
                        Assert.AreEqual("00001", diamond.ReportNumber);
                        Assert.AreEqual(1M, diamond.Caret);
                        Assert.AreEqual(ProductDiamondClarity.VS1, diamond.Clarity);
                        Assert.AreEqual(ProductDiamondColor.D, diamond.Color);
                        Assert.AreEqual(ProductDiamondCut.EX, diamond.Cut);
                        Assert.AreEqual(ProductDiamondPolish.EX, diamond.Polish);
                        Assert.AreEqual(ProductDiamondSymmetry.EX, diamond.Symmetry);
                        Assert.AreEqual("None", diamond.Fluorescence);
                        Assert.AreEqual(null, diamond.Comment);
                    }
                    break;
                case 2:
                    if (updated)
                    {
                        var diamond = diamonds.Single(d => d.Index == 2);
                        Assert.AreEqual(50000, diamond.Cost);
                        Assert.AreEqual(60000, diamond.SalePrice);
                        Assert.AreEqual("VM", diamond.Vendor);
                        Assert.AreEqual(ProductDiamondReport.EGL, diamond.ReportType);
                        Assert.AreEqual("00002", diamond.ReportNumber);
                        Assert.AreEqual(2M, diamond.Caret);
                        Assert.AreEqual(ProductDiamondClarity.VS2, diamond.Clarity);
                        Assert.AreEqual(ProductDiamondColor.E, diamond.Color);
                        Assert.AreEqual(ProductDiamondCut.VG, diamond.Cut);
                        Assert.AreEqual(ProductDiamondPolish.VG, diamond.Polish);
                        Assert.AreEqual(ProductDiamondSymmetry.VG, diamond.Symmetry);
                        Assert.AreEqual("Blue", diamond.Fluorescence);
                        Assert.AreEqual(null, diamond.Comment);
                    }
                    else
                    {
                        var diamond = diamonds.Single(d => d.Index == 2);
                        Assert.AreEqual(30000, diamond.Cost);
                        Assert.AreEqual(40000, diamond.SalePrice);
                        Assert.AreEqual("VM", diamond.Vendor);
                        Assert.AreEqual(ProductDiamondReport.EGL, diamond.ReportType);
                        Assert.AreEqual("00002", diamond.ReportNumber);
                        Assert.AreEqual(2M, diamond.Caret);
                        Assert.AreEqual(ProductDiamondClarity.VS2, diamond.Clarity);
                        Assert.AreEqual(ProductDiamondColor.E, diamond.Color);
                        Assert.AreEqual(ProductDiamondCut.VG, diamond.Cut);
                        Assert.AreEqual(ProductDiamondPolish.VG, diamond.Polish);
                        Assert.AreEqual(ProductDiamondSymmetry.VG, diamond.Symmetry);
                        Assert.AreEqual("Blue", diamond.Fluorescence);
                        Assert.AreEqual("Green", diamond.Comment);
                    }
                    break;
                case 3:
                    {
                        var diamond = diamonds.Single(d => d.Index == 3);
                        Assert.AreEqual(70000, diamond.Cost);
                        Assert.AreEqual(80000, diamond.SalePrice);
                        Assert.AreEqual("VM", diamond.Vendor);
                        Assert.AreEqual(ProductDiamondReport.IGI, diamond.ReportType);
                        Assert.AreEqual("00003", diamond.ReportNumber);
                        Assert.AreEqual(3M, diamond.Caret);
                        Assert.AreEqual(ProductDiamondClarity.VS2, diamond.Clarity);
                        Assert.AreEqual(ProductDiamondColor.E, diamond.Color);
                        Assert.AreEqual(ProductDiamondCut.VG, diamond.Cut);
                        Assert.AreEqual(ProductDiamondPolish.VG, diamond.Polish);
                        Assert.AreEqual(ProductDiamondSymmetry.VG, diamond.Symmetry);
                        Assert.AreEqual("Blue", diamond.Fluorescence);
                        Assert.AreEqual(null, diamond.Comment);
                    }
                    break;
            }
        }

        #endregion
    }
}
