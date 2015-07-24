namespace Home.Services.SsisService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;
    using Microsoft.SqlServer.Dts.Runtime;
    using Owin;

    public static class SsisServiceHelper
    {
        private static readonly IReadOnlyDictionary<string, SsisJobDescriptor> ssisJobDescriptors = new Dictionary<string, SsisJobDescriptor>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "sales",
                new SsisJobDescriptor()
                {
                    JobName = "Diamond_Sales",
                    EnvironmentName = "Sales",
                    PackageNames = new[] { "SalesLineItems" }
                }
            }
        };

        public static IReadOnlyDictionary<string, SsisJobDescriptor> SsisJobDescriptors
        {
            get { return ssisJobDescriptors; }
        }

        public static void ConfigureSsisService(this IAppBuilder app)
        {
            Debug.Assert(app != null);

            //// build EDM model

            var builder = new ODataConventionModelBuilder()
            {
                Namespace = typeof(SsisController).Namespace,
                ContainerName = "DefaultContainer"
            };

            builder.ComplexType<JobParameter>();
            builder.ComplexType<JobStatusResult>();
            builder.EnumType<JobStatus>();
            builder.EnumType<JobParameterType>();

            {
                var function = builder.Function("GetStatus");
                function.Parameter<string>("name").OptionalParameter = false;
                function.Returns<JobStatusResult>();
            }

            {
                var action = builder.Action("Run");
                action.Parameter<string>("name").OptionalParameter = false;
                action.CollectionParameter<JobParameter>("parameters").OptionalParameter = false;
                action.Returns<string>();
            }

            {
                var action = builder.Action("Stop");
                action.Parameter<string>("name").OptionalParameter = false;
                action.Returns<string>();
            }

            var edmModel = builder.GetEdmModel();

            //// configure web API

            var config = new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
            config.MapODataServiceRoute("SsisService", "ssisservice", edmModel);
            config.Services.Replace(typeof(IHttpControllerSelector), new ClientHttpControllerSelector(config, typeof(SsisServiceHelper)));
            config.EnsureInitialized();

            app.UseWebApi(config);
        }

        public static object Convert(string value, JobParameterType type)
        {
            Debug.Assert(!string.IsNullOrEmpty(value));
            Debug.Assert(!Enum.IsDefined(typeof(JobParameterType), value));

            switch (type)
            {
                case JobParameterType.Boolean: return bool.Parse(value);
                case JobParameterType.Byte: return byte.Parse(value);
                case JobParameterType.DateTime: return DateTime.Parse(value);
                case JobParameterType.Decimal: return decimal.Parse(value);
                case JobParameterType.Double: return double.Parse(value);
                case JobParameterType.Int16: return short.Parse(value);
                case JobParameterType.Int32: return int.Parse(value);
                case JobParameterType.Int64: return long.Parse(value);
                case JobParameterType.SByte: return sbyte.Parse(value);
                case JobParameterType.Single: return float.Parse(value);
                case JobParameterType.String: return value;
                case JobParameterType.UInt32: return uint.Parse(value);
                case JobParameterType.UInt64: return ulong.Parse(value);
            }

            throw new ArgumentException(string.Format("The value, {0}, and type, {1}, are not match.", value, type));
        }
    }
}