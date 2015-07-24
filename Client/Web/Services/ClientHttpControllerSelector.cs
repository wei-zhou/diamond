namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;

    public class ClientHttpControllerSelector : DefaultHttpControllerSelector
    {
        private readonly string serviceNamespace;

        public ClientHttpControllerSelector(HttpConfiguration configuration, Type type)
            : base(configuration)
        {
            Debug.Assert(type != null);

            this.serviceNamespace = type.Namespace;
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            var mapping = base.GetControllerMapping();

            foreach (var controllerName in mapping.Keys.ToArray())
            {
                var controllerType = mapping[controllerName].ControllerType;
                if (controllerType.Namespace != this.serviceNamespace)
                {
                    mapping.Remove(controllerName);
                }
            }

            return mapping;
        }
    }
}
