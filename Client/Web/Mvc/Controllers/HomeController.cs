namespace Home.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Home.Services.SecurityService;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [MvcHandleError]
    [MvcAuthorize]
    public class HomeController : Controller
    {
        private static readonly Lazy<IReadOnlyDictionary<string, ViewSetting>> allViewsLazy = new Lazy<IReadOnlyDictionary<string, ViewSetting>>(() =>
        {
            var result = new Dictionary<string, ViewSetting>();

            var array = ReadViewSettingsFile();
            foreach (var item in array)
            {
                var name = (string)item["name"];
                var setting = new ViewSetting()
                {
                    ViewName = (string)item["view"]
                };
                foreach (string role in (JArray)item["roles"])
                {
                    setting.Roles.Add(role);
                }

                result.Add(name, setting);
            }

            return result;
        }, LazyThreadSafetyMode.PublicationOnly);

        public ActionResult Index()
        {
            return View("~/Views/Home/Index.cshtml");
        }

        public ActionResult Page(string id)
        {
            if (id == null || !allViewsLazy.Value.ContainsKey(id))
            {
                return HttpNotFound();
            }

            var setting = allViewsLazy.Value[id];
            var user = this.Request.GetOwinContext().Authentication.User;
            if (!user.IsInRole(SecurityServiceHelper.RoleConstants.Administrator) && !setting.Roles.Any(role => user.IsInRole(role)))
            {
                return new HttpUnauthorizedResult();
            }

            return View(setting.ViewName);
        }

        // TODO: add output cache if possible
        public ActionResult Permissions()
        {
            var array = ReadViewSettingsFile();
            foreach (JObject item in array)
            {
                item.Property("view").Remove();
            }

            return Json(array.ToString(Formatting.None), "application/json", JsonRequestBehavior.AllowGet);
        }

        private static JArray ReadViewSettingsFile()
        {
            var path = HostingEnvironment.MapPath("~/app_data/views.json");
            using (var streamReader = new StreamReader(path, Encoding.UTF8))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    return JArray.Load(jsonReader);
                }
            }
        }

        private class ViewSetting
        {
            private readonly List<string> roles = new List<string>();

            public IList<string> Roles
            {
                get { return this.roles; }
            }

            public string ViewName { get; set; }
        }
    }
}