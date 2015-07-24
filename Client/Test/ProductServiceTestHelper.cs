namespace Home.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Fakes;
    using Home.Mvc.Models;
    using Home.Services.ProductService;
    using Microsoft.QualityTools.Testing.Fakes;
    using MvcProductController = Home.Mvc.Controllers.ProductController;

    public static class ProductServiceTestHelper
    {
        public static void UploadDiamond(string name, ProductFileUploadType type)
        {
            var stream = default(Stream);
            var context = default(IDisposable);
            try
            {
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(ProductServiceUnitTest), name);
                context = ShimsContext.Create();
                ShimHttpPostedFileWrapper.ConstructorHttpPostedFile = delegate { };
                var file = new StubHttpPostedFileWrapper(null)
                {
                    ContentLengthGet = () => (int)stream.Length,
                    InputStreamGet = () => stream
                };
                var controller = new MvcProductController();
                controller.UploadDiamondFile(file, type, false).GetAwaiter().GetResult();
            }
            finally
            {
                if (stream != null) stream.Dispose();
                if (context != null) context.Dispose();
            }
        }

        public static void CleanupDatabase()
        {
            using (var db = new ProductDbContext())
            {
                db.Database.ExecuteSqlCommand("DELETE tc_product_diamond");
                db.Database.ExecuteSqlCommand("DELETE tc_product_diamond_import");
                db.SaveChanges();
            }
        }
    }
}
