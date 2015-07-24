namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Results;
    using System.Web.OData;
    using System.Web.OData.Query;

    public static class ControllerHelper
    {
        public static IList<string> GetExpandPropertyNames<T>(this ODataQueryOptions<T> options)
            where T : class
        {
            var result = new List<string>();

            if (options != null && options.SelectExpand != null && !string.IsNullOrEmpty(options.SelectExpand.RawExpand))
            {
                var expands = options.SelectExpand.RawExpand.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var expand in expands)
                {
                    var item = expand.Trim();
                    var length = item.IndexOf('(');
                    if (length > -1)
                    {
                        item = item.Substring(0, length);
                    }
                    result.Add(item);
                }
            }

            return result;
        }

        public static object GetPropertyValueFromModel(object instance, string propertyName)
        {
            Debug.Assert(instance != null);
            Debug.Assert(!string.IsNullOrEmpty(propertyName));

            var propertyInfo = instance.GetType().GetTypeInfo().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new HttpException(string.Format("The property {0} is not found.", propertyName));
            }

            var propertyValue = propertyInfo.GetValue(instance, null);
            return propertyValue;
        }

        public static IHttpActionResult GetOKHttpActionResult(ODataController controller, object propertyValue)
        {
            Debug.Assert(controller != null);

            var okMethod = default(MethodInfo);
            var methods = controller.GetType().GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if (method.Name == "Ok" && method.GetParameters().Length == 1)
                {
                    okMethod = method;
                    break;
                }
            }

            okMethod = okMethod.MakeGenericMethod(propertyValue.GetType());
            var returnValue = okMethod.Invoke(controller, new object[] { propertyValue });
            return (IHttpActionResult)returnValue;
        }
    }
}
