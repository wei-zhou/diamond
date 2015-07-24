namespace Home.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    public class MvcHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            Debug.Assert(filterContext != null);

            MvcHandleErrorHelper.Handle(filterContext);

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                if (filterContext.ExceptionHandled)
                {
                    return;
                }

                var exception = filterContext.Exception;

                if (!this.ExceptionType.IsInstanceOfType(exception))
                {
                    return;
                }

                var statusCode = new HttpException(null, exception).GetHttpCode();
                if (statusCode != 500)
                {
                    return;
                }

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                base.OnException(filterContext);
            }
        }
    }
}