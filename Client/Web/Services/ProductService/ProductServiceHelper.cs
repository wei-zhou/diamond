namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.OData;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;
    using System.Xml.Linq;
    using Owin;

    public static class ProductServiceHelper
    {
        public static void ConfigureProductService(this IAppBuilder app)
        {
            Debug.Assert(app != null);

            //// build EDM model

            var builder = new ODataConventionModelBuilder()
            {
                Namespace = typeof(ProductController).Namespace,
                ContainerName = "DefaultContainer"
            };

            builder.EnumType<ProductDiamondClarity>();
            builder.EnumType<ProductDiamondColor>();
            builder.EnumType<ProductDiamondCut>();
            builder.EnumType<ProductDiamondPolish>();
            builder.EnumType<ProductDiamondReport>();
            builder.EnumType<ProductDiamondSymmetry>();
            builder.EnumType<ProductStatus>();
            builder.EntitySet<ProductDiamondImport>("DiamondImports");

            {
                var entityType = builder.EntitySet<ProductDiamond>("Diamonds").EntityType;
                entityType.Ignore(c => c._Import);
                entityType.Ignore(c => c._ImportId);
            }

            var edmModel = builder.GetEdmModel();

            //// configure web API

            var config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            config.MessageHandlers.Add(new ETagMessageHandler());
            config.MapODataServiceRoute("ProductService", "productservice", edmModel);
            config.Services.Replace(typeof(IHttpControllerSelector), new ClientHttpControllerSelector(config, typeof(ProductServiceHelper)));
            config.EnsureInitialized();

            app.UseWebApi(config);
        }
    }
}