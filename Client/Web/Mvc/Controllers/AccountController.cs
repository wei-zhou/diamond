namespace Home.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Home.Mvc.Models;
    using Home.Services.SecurityService;
    using Microsoft.Owin.Security;

    [MvcHandleError]
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            if (this.AuthenticationManager.User.Identity.IsAuthenticated)
            {
                return this.RedirectToLocal();
            }

            return this.View();
        }

        [HttpPost]
        [AjaxAwareValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                goto VALIDATION_ERROR;
            }

            var user = default(User);
            using (var db = new SecurityDbContext())
            {
                user = await db.Users.Where(u => u.LoginName == model.Name && u._Password == model.Password && !u._IsLocked).FirstOrDefaultAsync();
            }

            if (user == null)
            {
                goto LOGIN_ERROR;
            }

            var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString("D", CultureInfo.InvariantCulture)),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(CommonHelper.IdentityProviderClaimType, CommonHelper.IdentityProvider)
                },
                CommonHelper.DefaultAuthenticationType);
            this.AuthenticationManager.SignIn(identity);

            goto SUCCESS;

        VALIDATION_ERROR:

            if (this.Request.IsAjaxRequest())
            {
                return new HttpStatusCodeResult(HttpStatusCode.PreconditionFailed);
            }

            return this.View(model);

        LOGIN_ERROR:

            if (this.Request.IsAjaxRequest())
            {
                return new HttpStatusCodeResult(HttpStatusCode.PreconditionFailed);
            }

            this.ModelState.AddModelError(default(string), default(string));
            return this.View(model);

        SUCCESS:

            if (this.Request.IsAjaxRequest())
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return this.RedirectToLocal();
        }

        [MvcAuthorize]
        [HttpPost]
        [AjaxAwareValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            this.AuthenticationManager.SignOut(CommonHelper.DefaultAuthenticationType);

            if (this.Request.IsAjaxRequest())
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return this.RedirectToLocal();
        }

        public ActionResult CheckLogin()
        {
            var status = default(object);

            var user = this.AuthenticationManager.User;
            if (user.Identity.IsAuthenticated)
            {
                status = new { IsLoggedIn = true, Id = user.FindFirst(ClaimTypes.NameIdentifier).Value, Name = user.Identity.Name, Role = user.FindFirst(ClaimTypes.Role).Value };
            }
            else
            {
                status = new { IsLoggedIn = false };
            }

            return this.Json(status, JsonRequestBehavior.AllowGet);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return this.HttpContext.GetOwinContext().Authentication; }
        }

        private ActionResult RedirectToLocal()
        {
            var url = this.Request.QueryString["return_url"];
            if (this.Url.IsLocalUrl(url))
            {
                return this.Redirect(url);
            }

            return this.RedirectToRoute("Default");
        }
    }
}