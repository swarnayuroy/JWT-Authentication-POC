using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using web.Utils;
using web.Utils.CustomFilter;

namespace web.Controllers
{
    [Restrict]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*", Location = OutputCacheLocation.None)]
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult DashBoard()
        {
            var claimsPrincipal = new ClaimsPrincipal();

            string sessionToken = Request.Cookies["sessionToken"]?.Value;
            if (!String.IsNullOrEmpty(sessionToken))
            {
                claimsPrincipal = JwtHelper.DecodeToken(sessionToken);
                string userId = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (!String.IsNullOrEmpty(userId))
                {
                    // Set cache control headers to prevent back navigation
                    Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Cache.SetNoStore();
                    Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                    Response.AppendHeader("Pragma", "no-cache");

                    return View();
                }
                return RedirectToAction("Logout");
            }
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Logout()
        {
            try
            {
                var cookie = Request.Cookies["sessionToken"];
                if (cookie != null)
                {
                    var claimsPrincipal = JwtHelper.DecodeToken(cookie.Value);
                    string userId = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                    cookie.Expires = DateTime.Now.AddMinutes(-1);
                    cookie.Value = String.Empty;
                    Response.Cookies.Add(cookie);
                    Request.Cookies.Remove("sessionToken");
                }

                Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                Response.AppendHeader("Pragma", "no-cache");
                Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate");

                return RedirectToAction("Login", "Account");
            }
            catch (Exception)
            {
                //_logger.LogDetails(LogType.ERROR, ex.Message);
                return RedirectToAction("Login", "Account");
            }

        }
    }
}