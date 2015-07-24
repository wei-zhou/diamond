namespace Home.Services.SaleService
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

    public static class SaleServiceHelper
    {
        public static void ConfigureSalesService(this IAppBuilder app)
        {
            Debug.Assert(app != null);

            //// build EDM model

            var builder = new ODataConventionModelBuilder()
            {
                Namespace = typeof(SalesController).Namespace,
                ContainerName = "DefaultContainer"
            };

            builder.EnumType<ContactMethod>();
            builder.EnumType<SaleStatus>();

            {
                var entityType = builder.EntityType<Contact>();
                entityType.Ignore(c => c._SaleHeaderId);
                entityType.Ignore(c => c._SaleHeader);
            }

            {
                var entityType = builder.EntityType<SaleLineItem>();
                entityType.Ignore(c => c._DynamicProperties);
                entityType.Ignore(c => c._SaleHeaderId);
                entityType.Ignore(c => c._SaleHeader);
            }

            {
                builder.EntitySet<SaleHeader>("Sales");
            }

            var edmModel = builder.GetEdmModel();

            //// configure web API

            var config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            config.MessageHandlers.Add(new ETagMessageHandler());
            config.MapODataServiceRoute("SalesService", "salesservice", edmModel);
            config.Services.Replace(typeof(IHttpControllerSelector), new ClientHttpControllerSelector(config, typeof(SaleServiceHelper)));
            config.EnsureInitialized();

            app.UseWebApi(config);
        }

        public static string GenerateSaleNumber(this SaleHeader sale)
        {
            Debug.Assert(sale != null);

            return string.Concat(
                sale.Created.Year.ToString("0000", CultureInfo.InvariantCulture),
                sale.Created.Month.ToString("00", CultureInfo.InvariantCulture),
                sale.Created.Day.ToString("00", CultureInfo.InvariantCulture),
                sale.DayNumber.ToString("00", CultureInfo.InvariantCulture),
                sale.TotalNumber.ToString("000000", CultureInfo.InvariantCulture));
        }
    }
}
