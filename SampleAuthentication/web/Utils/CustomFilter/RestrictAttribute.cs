using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace web.Utils.CustomFilter
{
    public class RestrictAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var cookie = filterContext.HttpContext.Request.Cookies["sessionToken"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                    { "controller", "Account" },
                    { "action", "Login" }
                });
            }
            base.OnActionExecuting(filterContext);
        }
    }
}