using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using web.Models;
using web.Models.ResponseModel;
using web.Models.SessionModel;
using web.Repository;
using web.Utils;
using web.Utils.CustomFilter;

namespace web.Controllers
{
    [Restrict]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*", Location = OutputCacheLocation.None)]
    public class HomeController : Controller
    {
        private readonly IWebRepository _repository;
        public HomeController(IWebRepository repository)
        {
            this._repository = repository;
        }
        // GET: Home
        public async Task<ActionResult> DashBoard()
        {
            var claimsPrincipal = new ClaimsPrincipal();

            string sessionToken = Request.Cookies["sessionToken"]?.Value;
            if (!String.IsNullOrEmpty(sessionToken))
            {
                claimsPrincipal = JwtHelper.DecodeToken(sessionToken);
                string userId = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (!String.IsNullOrEmpty(userId))
                {
                    ResponseDetail response = new ResponseDetail();

                    // Set cache control headers to prevent back navigation
                    await SetCacheControl();

                    response = await _repository.GetUserDetail(sessionToken, userId);
                    if (response.Status)
                    {
                        if ((response is ResponseDataDetail<UserDetail> userDetailResponse) && (userDetailResponse.Data != null))
                        {
                            return View("DashBoard", new UserSessionDetail
                            {
                                User = userDetailResponse.Data,
                                ToastNotification = new ToastNotification
                                {
                                    IsEnable = false,
                                }
                            });
                        }
                    }
                    return View();
                }
                return RedirectToAction("Logout");
            }
            return RedirectToAction("Login", "Account");
        }

        public async Task<ActionResult> Logout()
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
                await SetCacheControl();
                Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate");

                return RedirectToAction("Login", "Account");
            }
            catch (Exception)
            {
                //_logger.LogDetails(LogType.ERROR, ex.Message);
                return RedirectToAction("Login", "Account");
            }

        }

        public Task SetCacheControl()
        {
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.AppendHeader("Pragma", "no-cache");

            return Task.CompletedTask;
        }
    }
}