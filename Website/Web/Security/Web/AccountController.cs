namespace Home.Security.Web
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Owin.Security;

    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View(Common.View.VirtualPath.Login);
        }

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult Login(string provider, string return_url)
        //{
        //    switch (provider)
        //    {
        //        case Common.IdentityProvider.Name.QQ:
        //            {
        //                break;
        //            }
        //        case Common.IdentityProvider.Name.Sina:
        //            {
        //                break;
        //            }
        //        case Common.IdentityProvider.Name.WeiXin:
        //            {
        //                break;
        //            }
        //        default:
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //}

        //[AllowAnonymous]
        //public async Task<ActionResult> LoginCallback(string return_url)
        //{
        //    throw new NotImplementedException();
        //}

        [AllowAnonymous]
        public ActionResult LoginFailure()
        {
            return View(Common.View.VirtualPath.LoginFailure);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            // TODO: add signout code
            return Redirect("/");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }
    }
}