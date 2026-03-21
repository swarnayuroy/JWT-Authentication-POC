using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
                            // Store user in session
                            Session["currentUser"] = userDetailResponse.Data;
                            if (userDetailResponse.Data.IsAdmin)
                            {
                                ResponseDetail sessionDataResponse = await _repository.GetAllUser(sessionToken, userId, 1);
                                if (sessionDataResponse.Status)
                                {
                                    if ((sessionDataResponse is ResponseDataDetail<PagedResult<UserDetail>> pagedUsers) && (pagedUsers.Data != null))
                                    {
                                        return View("DashBoard", new AdminSessionDetail
                                        {
                                            User = userDetailResponse.Data,
                                            Data = new SessionData
                                            {
                                                Users = pagedUsers.Data.Items,
                                                UserCount = pagedUsers.Data.ItemCount,
                                                PageSize = pagedUsers.Data.PageSize,
                                                TotalPages = pagedUsers.Data.TotalPages,
                                                CurrentPage = pagedUsers.Data.CurrentPage,
                                            },
                                            ToastNotification = new ToastNotification
                                            {
                                                IsEnable = false,
                                            }
                                        });
                                    }
                                }
                                return View("DashBoard", new AdminSessionDetail
                                {
                                    User = userDetailResponse.Data,                                    
                                    ToastNotification = new ToastNotification
                                    {
                                        IsEnable = true,
                                        Type = sessionDataResponse.StatusCode != null ? (HttpStatusCode)sessionDataResponse.StatusCode : HttpStatusCode.BadRequest,
                                        StatusIcon = ToastNotification.WARNING_ICON,
                                        Message = "Oops! failed to fetch users."
                                    }
                                });
                            }
                            
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
                }
                return RedirectToAction("Logout");
            }
            return RedirectToAction("Login", "Account");
        }

        // GET: Home/PaginateOperation/{page}/{searchText}
        public async Task<ActionResult> PaginateOperation(int page = 1, string searchText = "")
        {
            await SetCacheControl();
            if (page < 1) 
            {
                return RedirectToAction("DashBoard");
            }

            string sessionToken = Request.Cookies["sessionToken"]?.Value;
            UserDetail currentUser = Session["currentUser"] as UserDetail;
            if (!String.IsNullOrEmpty(sessionToken) && currentUser != null)
            {
                if (currentUser.IsAdmin)
                {
                    ResponseDetail sessionDataResponse = string.IsNullOrEmpty(searchText) ? await _repository.GetAllUser(sessionToken, currentUser.Id, page) : 
                        await _repository.GetUsersBySearch(sessionToken, currentUser.Id, searchText, page);
                    if (sessionDataResponse.Status)
                    {
                        if ((sessionDataResponse is ResponseDataDetail<PagedResult<UserDetail>> pagedUsers) && (pagedUsers.Data != null))
                        {
                            if (page > pagedUsers.Data.TotalPages)
                            {
                                return RedirectToAction("DashBoard");
                            }
                            return View("DashBoard", new AdminSessionDetail
                            {
                                User = currentUser,
                                Data = new SessionData
                                {
                                    Users = pagedUsers.Data.Items,
                                    UserCount = pagedUsers.Data.ItemCount,
                                    PageSize = pagedUsers.Data.PageSize,
                                    TotalPages = pagedUsers.Data.TotalPages,
                                    CurrentPage = pagedUsers.Data.CurrentPage,
                                },
                                ToastNotification = new ToastNotification
                                {
                                    IsEnable = false,
                                }
                            });
                        }
                    }
                    return View("DashBoard", new AdminSessionDetail
                    {
                        User = currentUser,
                        ToastNotification = new ToastNotification
                        {
                            IsEnable = true,
                            Type = sessionDataResponse.StatusCode != null ? (HttpStatusCode)sessionDataResponse.StatusCode : HttpStatusCode.BadRequest,
                            StatusIcon = ToastNotification.WARNING_ICON,
                            Message = "Oops! failed to fetch users."
                        }
                    });
                }
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
                    Request.Cookies.Remove("currentUser");
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