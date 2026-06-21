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
using web.Models.SessionModel.Modal;
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

        // GET: Home/DashBoard
        [HttpGet]
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

                    response = await _repository.GetUser(sessionToken, userId);

                    if (response.Status)
                    {
                        if ((response is ResponseDataDetail<UserDetail> userDetailResponse) && (userDetailResponse.Data != null))
                        {
                            // Store user in session
                            Session["currentUser"] = userDetailResponse.Data;
                            if (userDetailResponse.Data.IsAdmin)
                            {
                                ResponseDetail sessionDataResponse = await _repository.GetAllUser(sessionToken, userId, "User", 1);
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
                                        Message = !string.IsNullOrEmpty(sessionDataResponse.Message) ? sessionDataResponse.Message : "Oops! failed to fetch users."
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

        // GET: Home/PaginateOperation?page={page}&searchText={searchText}
        [HttpGet]
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
                    // Initialize notifications list
                    List<ToastNotification> notificationsList = new List<ToastNotification>();

                    /*
                     * Once admin performs delete operation then only one notification will be there in 
                     * TempData with key "DeleteNotification" and it will be added to notificationsList.
                    */
                    if (TempData["DeleteNotification"] != null)
                    {
                        notificationsList.Add((ToastNotification)TempData["DeleteNotification"]);
                    }
                    
                    ResponseDetail sessionDataResponse = string.IsNullOrEmpty(searchText) ? await _repository.GetAllUser(sessionToken, currentUser.Id, "User", page) : 
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
                                ToastNotification = notificationsList.Count == 1 ? notificationsList[0] : new ToastNotification
                                {
                                    IsEnable = false,
                                },
                                EnabledNotifications = notificationsList.Count > 1 ? notificationsList : null
                            });
                        }
                    }

                    notificationsList.Add(new ToastNotification
                    {
                        IsEnable = true,
                        Type = sessionDataResponse.StatusCode != null ? (HttpStatusCode)sessionDataResponse.StatusCode : HttpStatusCode.BadRequest,
                        StatusIcon = ToastNotification.WARNING_ICON,
                        Message = "Oops! failed to fetch users."
                    });
                    
                    return View("DashBoard", new AdminSessionDetail
                    {
                        User = currentUser,
                        ToastNotification = notificationsList.Count == 1 ? notificationsList[0] : new ToastNotification
                        {
                            IsEnable = true,
                            Type = sessionDataResponse.StatusCode != null ? (HttpStatusCode)sessionDataResponse.StatusCode : HttpStatusCode.BadRequest,
                            StatusIcon = ToastNotification.WARNING_ICON,
                            Message = "Oops! failed to fetch users."
                        },
                        EnabledNotifications = notificationsList.Count > 1 ? notificationsList : null
                    });
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        // GET: Home/ViewAdmins
        [HttpGet]
        public async Task<ActionResult> ViewAdmins()
        {
            await SetCacheControl();
            string sessionToken = Request.Cookies["sessionToken"]?.Value;
            UserDetail currentUser = Session["currentUser"] as UserDetail;

            if (!string.IsNullOrEmpty(sessionToken) && currentUser !=null)
            {
                if (currentUser.IsAdmin)
                {
                    ResponseDetail response = await _repository.GetAllUser(sessionToken, currentUser.Id, "Admin", 1);
                    if (response.Status && response is ResponseDataDetail<AdminResult> adminResultSet)
                    {
                        return PartialView("_ViewAdminsModal", new AdminModal { ModalData = adminResultSet.Data });
                    }
                }
                return PartialView("_UserDetailError");
            }
            return RedirectToAction("Logout", "Home");
        }

        // GET: Home/ViewUser?userId={userId}
        [HttpGet]
        public async Task<ActionResult> ViewUser(string userId)
        {
            await SetCacheControl();
            string sessionToken = Request.Cookies["sessionToken"]?.Value;
            UserDetail currentUser = Session["currentUser"] as UserDetail;

            if (!String.IsNullOrEmpty(sessionToken) && currentUser != null) {
                if (currentUser.IsAdmin) 
                {
                    ResponseDetail response = await _repository.GetUserDetail(sessionToken, userId);
                    if (response.Status)
                    {
                        if (response is ResponseDataDetail<FullUserDetail> userDetail)
                        {
                            return PartialView("_UserDetailModal", userDetail.Data);
                        }
                    }
                    return PartialView("_UserDetailError");
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        // GET: Home/DeleteUser?userId={userId}
        [HttpGet]
        public async Task<ActionResult> DeleteUser(string userId)
        {
            await SetCacheControl();
            string sessionToken = Request.Cookies["sessionToken"]?.Value;
            UserDetail currentUser = Session["currentUser"] as UserDetail;
            if (!String.IsNullOrEmpty(sessionToken) && currentUser != null)
            {
                if (currentUser.IsAdmin)
                {
                    ResponseDetail response = await _repository.GetUser(sessionToken, userId);
                    if (response.Status) 
                    {
                        if (response is ResponseDataDetail<UserDetail> userDetail)
                        {
                            return PartialView("_DeleteUserModal", userDetail.Data);
                        }
                    }
                    return PartialView("_UserDetailError");
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        public async Task<ActionResult> ConfirmDelete(string userId)
        {
            await SetCacheControl();
            string sessionToken = Request.Cookies["sessionToken"]?.Value;
            UserDetail currentUser = Session["currentUser"] as UserDetail;

            if (!String.IsNullOrEmpty(sessionToken) && currentUser != null)
            {
                if (currentUser.IsAdmin)
                {
                    ResponseDetail response = await _repository.DeleteAccount(sessionToken, userId);
                    if (response.Status)
                    {
                        TempData["DeleteNotification"] = new ToastNotification
                        {
                            IsEnable = true,
                            Type = HttpStatusCode.OK,
                            StatusIcon = ToastNotification.SUCCESS_ICON,
                            Message = response.Message
                        };
                    }
                    else
                    {
                        TempData["DeleteNotification"] = new ToastNotification
                        {
                            IsEnable = true,
                            Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.BadRequest,
                            StatusIcon = ToastNotification.WARNING_ICON,
                            Message = response.Message
                        };                       
                    }
                    return RedirectToAction("PaginateOperation", "Home", new { page = 1, searchText = "" });
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        public async Task<ActionResult> VerifyAccount(string value, bool haveOtpValue = false)
        {
            ResponseDetail response = new ResponseDetail();
            UserDetail currentUser = Session["currentUser"] as UserDetail;            

            if (haveOtpValue)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    VerifyAccount detail = new VerifyAccount
                    {
                        Email = currentUser.Email,
                        Otp = value,
                        IsLoggedIn = true
                    };
                    response = await _repository.VerifyAccount(detail);
                    if (response.Status)
                    {
                        return RedirectToAction("Dashboard", "Home");
                    }
                    return View("Dashboard", new UserSessionDetail
                    {
                        User = currentUser,
                        Verify = new VerifyUser
                        {
                            showOTP_Field = true,
                            OTP_Field = new VerifyOTP()
                        },
                        ToastNotification = new ToastNotification
                        {
                            IsEnable = true,
                            Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.BadRequest,
                            StatusIcon = ToastNotification.WARNING_ICON,
                            Message = response.Message
                        }
                    });
                }

                // OTP input field is empty as user didn't type the OTP
                return View("Dashboard", new UserSessionDetail
                {
                    User = currentUser,
                    Verify = new VerifyUser
                    {
                        showOTP_Field = true,
                        OTP_Field = new VerifyOTP()
                    },
                    ToastNotification = new ToastNotification
                    {
                        IsEnable = true,
                        Type = HttpStatusCode.BadRequest,
                        StatusIcon = ToastNotification.WARNING_ICON,
                        Message = "Please enter the OTP"
                    }
                });
            }
            else
            {
                if (!string.IsNullOrEmpty(value))
                {
                    VerifyUser verification = new VerifyUser
                    {
                        showOTP_Field = true,
                        OTP_Field = new VerifyOTP()
                    };
                    CheckEmail userEmail = new CheckEmail { Email = currentUser.Email };
                    response = await _repository.CheckEmail(userEmail);

                    if (response.Status)
                    {                        
                        return View("Dashboard", new UserSessionDetail
                        {
                            User = currentUser,
                            Verify = verification,
                            ToastNotification = new ToastNotification
                            {
                                IsEnable = true,
                                Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.OK,
                                StatusIcon = ToastNotification.SUCCESS_ICON,
                                Message = "OTP has been sent to your email address."
                            }
                        });
                    }
                    return View("Dashboard", new UserSessionDetail
                    {
                        User = currentUser,
                        ToastNotification = new ToastNotification
                        {
                            IsEnable = true,
                            Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.BadRequest,
                            StatusIcon = ToastNotification.WARNING_ICON,
                            Message = response.Message
                        }
                    });
                }
            }
            return RedirectToAction("Logout", "Home");
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