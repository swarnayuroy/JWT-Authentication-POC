using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using web.Models;
using web.Models.ResponseModel;
using web.Repository;

namespace web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWebRepository _repository;
        public AccountController(IWebRepository repository)
        {
            this._repository = repository;
        }
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            // Set cache control headers to prevent back navigation
            Task.Run(() => SetCacheControl());

            return View(new Form
            {
                showSignInForm = true,
                showSignUpForm = false,
                showForgotPasswordForm = false,
                ToastNotification = new ToastNotification
                {
                    IsEnable = false,
                }
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Form formModel)
        {
            ResponseDetail response = new ResponseDetail();
            if (!ModelState.IsValid)
            {
                return View(new Form
                {
                    SignIn = formModel.SignIn,
                    showSignInForm = true,
                    showSignUpForm = false,
                    showForgotPasswordForm = false,
                    ToastNotification = new ToastNotification
                    {
                        IsEnable = false,
                    }
                });
            }
            
            response = await _repository.CheckCredential(formModel.SignIn);
            if (response.Status)
            {
                if (response is ResponseDataDetail<string> responseWithToken && !string.IsNullOrEmpty(responseWithToken.Data))
                {
                    var cookie = new HttpCookie("sessionToken", responseWithToken.Data)
                    {
                        HttpOnly = true,
                        Secure = true
                    };
                    Response.Cookies.Add(cookie);
                    return RedirectToAction("Dashboard", "Home");
                }
                return View("Login", new Form
                {
                    showSignInForm = true,
                    showSignUpForm = false,
                    showForgotPasswordForm = false,
                    ToastNotification = new ToastNotification
                    {
                        IsEnable = true,
                        Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.BadRequest,
                        StatusIcon = ToastNotification.WARNING_ICON,
                        Message = "Oops! please try again later."
                    }
                });
            }

            return View("Login", new Form
            {
                showSignInForm = true,
                showSignUpForm = false,
                showForgotPasswordForm = false,
                ToastNotification = new ToastNotification
                {
                    IsEnable = true,
                    Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.BadRequest,
                    StatusIcon = ToastNotification.WARNING_ICON,
                    Message = response.Message
                }
            });
        }

        // GET: SignUp
        [HttpGet]
        public ActionResult SignUp()
        {
            return View("Login", new Form
            {
                showSignInForm = false,
                showSignUpForm = true,
                showForgotPasswordForm = false,
                ToastNotification = new ToastNotification
                {
                    IsEnable = false,
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignUp(Form formModel) 
        {             
            ResponseDetail response = new ResponseDetail();
            if (!ModelState.IsValid)
            {
                return View("Login", new Form
                {
                    SignUp = formModel.SignUp,
                    showSignInForm = false,
                    showSignUpForm = true,
                    showForgotPasswordForm = false,
                    ToastNotification = new ToastNotification
                    {
                        IsEnable = false,
                    }
                });
            }
            response = await _repository.RegisterUser(formModel.SignUp);
            if (response.Status)
            {
                return View("Login", new Form
                {
                    showSignInForm = true,
                    showSignUpForm = false,
                    showForgotPasswordForm = false,
                    ToastNotification = new ToastNotification
                    {
                        IsEnable = true,
                        Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.OK,
                        StatusIcon = ToastNotification.SUCCESS_ICON,
                        Message = response.Message
                    }
                });
            }
            return View("Login", new Form
            {
                showSignInForm = false,
                showSignUpForm = true,
                showForgotPasswordForm = false,
                ToastNotification = new ToastNotification
                {
                    IsEnable = true,
                    Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.BadRequest,
                    StatusIcon = ToastNotification.WARNING_ICON,
                    Message = response.Message
                }
            });
        }

        // GET: ForgotPassword
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View("Login", new Form
            {
                showSignInForm = false,
                showSignUpForm = false,
                showForgotPasswordForm = true,
                Forgot = new ForgotPassword
                {
                    showEmail_Field = true,
                    showOTP_Field = false,
                    showSetPassword_Field = false,
                    Email_Field = new CheckEmail()
                },
                ToastNotification = new ToastNotification
                {
                    IsEnable = false,
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(Form formModel)
        {   
            if (formModel.showForgotPasswordForm)
            {
                ResponseDetail response = new ResponseDetail();
                ForgotPassword forgotPassword = new ForgotPassword();
                if (!ModelState.IsValid)
                {                    
                    if (formModel.Forgot.showEmail_Field)
                    {
                        forgotPassword = new ForgotPassword
                        {
                            showEmail_Field = true,
                            showOTP_Field = false,
                            showSetPassword_Field = false,
                            Email_Field = formModel.Forgot.Email_Field
                        };
                    }
                    else if (formModel.Forgot.showOTP_Field)
                    {
                        forgotPassword = new ForgotPassword
                        {
                            showEmail_Field = false,
                            showOTP_Field = true,
                            showSetPassword_Field = false,
                            Email_Field = formModel.Forgot.Email_Field,
                            OTP_Field = formModel.Forgot.OTP_Field
                        };
                    }
                    else
                    {
                        forgotPassword = new ForgotPassword
                        {
                            showEmail_Field = false,
                            showOTP_Field = false,
                            showSetPassword_Field = true,
                            Email_Field = formModel.Forgot.Email_Field,
                            SetPassword_Field = formModel.Forgot.SetPassword_Field
                        };
                    }

                    return View("Login", new Form
                    {
                        showSignInForm = false,
                        showSignUpForm = false,
                        showForgotPasswordForm = true,
                        Forgot = forgotPassword,
                        ToastNotification = new ToastNotification
                        {
                            IsEnable = false,
                        }
                    });
                }

                #region Step 1: checking for valid email and enabling OTP field
                if (formModel.Forgot.showEmail_Field)
                {
                    CheckEmail userEmail = formModel.Forgot.Email_Field;
                    response = await _repository.CheckEmail(userEmail);
                    if (response.Status)
                    {
                        forgotPassword = new ForgotPassword
                        {
                            showEmail_Field = false,
                            showOTP_Field = true,
                            showSetPassword_Field = false,
                            Email_Field = userEmail,
                            OTP_Field = new VerifyOTP()
                        };

                        return View("Login", new Form
                        {
                            showSignInForm = false,
                            showSignUpForm = false,
                            showForgotPasswordForm = true,
                            Forgot = forgotPassword,
                            ToastNotification = new ToastNotification
                            {
                                IsEnable = true,
                                Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.OK,
                                StatusIcon = ToastNotification.SUCCESS_ICON,
                                Message = "OTP has been sent to your email address."
                            }
                        });
                    }

                    // stay on email field if email is invalid or any error occurs
                    forgotPassword = new ForgotPassword
                    {
                        showEmail_Field = true,
                        showOTP_Field = false,
                        showSetPassword_Field = false,
                        Email_Field = userEmail
                    };

                    return View("Login", new Form
                    {
                        showSignInForm = false,
                        showSignUpForm = false,
                        showForgotPasswordForm = true,
                        Forgot = forgotPassword,
                        ToastNotification = new ToastNotification
                        {
                            IsEnable = true,
                            Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.BadRequest,
                            StatusIcon = ToastNotification.WARNING_ICON,
                            Message = response.Message
                        }
                    });
                }
                #endregion

                #region Step 2: checking for valid OTP and enabling set password field
                if (formModel.Forgot.showOTP_Field)
                {
                    string email = formModel.Forgot.Email_Field.Email;
                    string otp = formModel.Forgot.OTP_Field.OTP;
                    VerifyAccount detail = new VerifyAccount
                    {
                        Email = email,
                        Otp = otp
                    };

                    response = await _repository.VerifyAccount(detail);
                    if (response.Status)
                    {
                        forgotPassword = new ForgotPassword
                        {
                            showEmail_Field = false,
                            showOTP_Field = false,
                            showSetPassword_Field = true,
                            Email_Field = new CheckEmail { Email = email },
                            SetPassword_Field = new SetNewPassword()
                        };

                        return View("Login", new Form
                        {
                            showSignInForm = false,
                            showSignUpForm = false,
                            showForgotPasswordForm = true,
                            Forgot = forgotPassword,
                            ToastNotification = new ToastNotification
                            {
                                IsEnable = false
                            }
                        });
                    }

                    // stay on otp field if otp is invalid or any error occurs
                    forgotPassword = new ForgotPassword
                    {
                        showEmail_Field = false,
                        showOTP_Field = true,
                        showSetPassword_Field = false,
                        Email_Field = new CheckEmail { Email = email },
                        OTP_Field = new VerifyOTP()
                    };

                    return View("Login", new Form
                    {
                        showSignInForm = false,
                        showSignUpForm = false,
                        showForgotPasswordForm = true,
                        Forgot = forgotPassword,
                        ToastNotification = new ToastNotification
                        {
                            IsEnable = true,
                            Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.BadRequest,
                            StatusIcon = ToastNotification.WARNING_ICON,
                            Message = response.Message
                        }
                    });
                }
                #endregion

                #region Step 3: setting new password by the user
                if (formModel.Forgot.showSetPassword_Field)
                {
                    string email = formModel.Forgot.Email_Field.Email;
                    Credential credential = new Credential
                    {
                        Email = email,
                        Password = formModel.Forgot.SetPassword_Field.NewPassword
                    };

                    response = await _repository.SetNewPassword(credential);

                    if (response.Status)
                    {
                        return View("Login", new Form
                        {
                            showSignInForm = true,
                            showSignUpForm = false,
                            showForgotPasswordForm = false,
                            ToastNotification = new ToastNotification
                            {
                                IsEnable = true,
                                Type = response.StatusCode != null ? (HttpStatusCode)response.StatusCode : HttpStatusCode.OK,
                                StatusIcon = ToastNotification.SUCCESS_ICON,
                                Message = response.Message
                            }
                        });
                    }

                    // stay on password field if any error occurs while setting new password
                    forgotPassword = new ForgotPassword
                    {
                        showEmail_Field = false,
                        showOTP_Field = false,
                        showSetPassword_Field = true,
                        Email_Field = new CheckEmail { Email = email },
                        SetPassword_Field = new SetNewPassword()
                    };

                    return View("Login", new Form
                    {
                        showSignInForm = false,
                        showSignUpForm = false,
                        showForgotPasswordForm = true,
                        Forgot = forgotPassword,
                        ToastNotification = new ToastNotification
                        {
                            IsEnable = false
                        }
                    });
                }
                #endregion
            }
            return View(new Form
            {
                showSignInForm = true,
                showSignUpForm = false,
                showForgotPasswordForm = false,
                ToastNotification = new ToastNotification
                {
                    IsEnable = false,
                }
            });
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