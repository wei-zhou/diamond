namespace Home.Test
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Fakes;
    using Home.Mvc.Models;
    using Home.Services.ProductService;
    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;
    using MvcProductController = Home.Mvc.Controllers.ProductController;

    [TestClass]
    public class ProductServiceE2ETest : BaseTest
    {
        #region Initialize & cleanup

        private static readonly string ProductServiceRootUrl = ConfigurationManager.AppSettings["ProductServiceRootUrl"];

        [TestInitialize]
        public void TestInitialize()
        {
            ProductServiceTestHelper.CleanupDatabase();
            ProductServiceTestHelper.UploadDiamond("product_diamond_1.csv", ProductFileUploadType.Replace);
            ProductServiceTestHelper.UploadDiamond("product_diamond_3.csv", ProductFileUploadType.Append);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ProductServiceTestHelper.CleanupDatabase();
        }

        #endregion

        #region Diamond imports - GET

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetDiamondImports()
        {
            var tokens = ODataClientHelper.GetServerPaging(ProductServiceRootUrl + "DiamondImports");
            Assert.AreEqual(2, tokens.Count);
            var imports = default(IList<ProductDiamondImport>);
            using (var db = new ProductDbContext())
            {
                imports = db.DiamondImports.ToArray();
            }

            CompareCollection(imports, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetDiamondImports_Expand()
        {
            var tokens = ODataClientHelper.GetServerPaging(ProductServiceRootUrl + "DiamondImports?$expand=Products");
            Assert.AreEqual(2, tokens.Count);
            var imports = default(IList<ProductDiamondImport>);
            using (var db = new ProductDbContext())
            {
                imports = db.DiamondImports.Include("Products").ToArray();
            }

            CompareCollection(imports, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetDiamondImportById()
        {
            var import = default(ProductDiamondImport);
            using (var db = new ProductDbContext())
            {
                import = db.DiamondImports.First();
            }
            var token = ODataClientHelper.InvokeGet(string.Format("{0}DiamondImports({1})", ProductServiceRootUrl, import.Id));
            Compare(import, token);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetDiamondImportProperty()
        {
            var import = default(ProductDiamondImport);
            using (var db = new ProductDbContext())
            {
                import = db.DiamondImports.First();
            }
            var token = ODataClientHelper.InvokeGet(string.Format("{0}DiamondImports({1})/Count", ProductServiceRootUrl, import.Id));
            Assert.AreEqual(import.Count, (int)token);
        }

        #endregion

        #region Diamond - GET

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetDiamonds()
        {
            var tokens = ODataClientHelper.GetServerPaging(ProductServiceRootUrl + "Diamonds");
            Assert.AreEqual(3, tokens.Count);
            var diamonds = default(IList<ProductDiamond>);
            using (var db = new ProductDbContext())
            {
                diamonds = db.Diamonds.ToArray();
            }

            CompareCollection(diamonds, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetDiamondById()
        {
            var diamond = default(ProductDiamond);
            using (var db = new ProductDbContext())
            {
                diamond = db.Diamonds.First();
            }
            var token = ODataClientHelper.InvokeGet(string.Format("{0}Diamonds({1})", ProductServiceRootUrl, diamond.Id));
            Compare(diamond, token);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetDiamondProperty()
        {
            var diamond = default(ProductDiamond);
            using (var db = new ProductDbContext())
            {
                diamond = db.Diamonds.First();
            }
            var token = ODataClientHelper.InvokeGet(string.Format("{0}Diamonds({1})/Cost", ProductServiceRootUrl, diamond.Id));
            Assert.AreEqual(diamond.Cost, (decimal)token);
        }

        #endregion

        #region Diamond imports helper

        private static void Compare(ProductDiamondImport import, JToken token)
        {
            CompareCommon(import, token);
            Assert.AreEqual(import.Count, (int)token["Count"]);
            CompareLazyProperty(() => import.Products, token, "Products", Compare);
        }

        #endregion

        #region Diamond helper

        private static void Compare(ProductDiamond diamond, JToken token)
        {
            CompareCommon(diamond, token);
            Assert.AreEqual(diamond.Cost, (decimal)token["Cost"]);
            Assert.AreEqual(diamond.SalePrice, (decimal)token["SalePrice"]);
            Assert.AreEqual(diamond.Vendor, (string)token["Vendor"]);
            Assert.AreEqual(diamond.ReportType, ConvertToEnum<ProductDiamondReport>(token["ReportType"]));
            Assert.AreEqual(diamond.ReportNumber, (string)token["ReportNumber"]);
            Assert.AreEqual(diamond.Caret, (decimal?)token["Caret"]);
            Assert.AreEqual(diamond.Clarity, ConvertToEnum<ProductDiamondClarity>(token["Clarity"]));
            Assert.AreEqual(diamond.Color, ConvertToEnum<ProductDiamondColor>(token["Color"]));
            Assert.AreEqual(diamond.Cut, ConvertToEnum<ProductDiamondCut>(token["Cut"]));
            Assert.AreEqual(diamond.Polish, ConvertToEnum<ProductDiamondPolish>(token["Polish"]));
            Assert.AreEqual(diamond.Symmetry, ConvertToEnum<ProductDiamondSymmetry>(token["Symmetry"]));
            Assert.AreEqual(diamond.Fluorescence, (string)token["Fluorescence"]);
            Assert.AreEqual(diamond.Comment, (string)token["Comment"]);
        }

        #endregion
    }
}
