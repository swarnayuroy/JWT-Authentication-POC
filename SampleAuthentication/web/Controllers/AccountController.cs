using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net;
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
        public ActionResult Login()
        {
            return View(new Form
            {
                showSignInForm = true,
                showSignUpForm = false,
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
                    ToastNotification = new ToastNotification
                    {
                        IsEnable = false,
                    }
                });
            }
            
            response = await _repository.CheckCredential(formModel.SignIn);
            if (response.Status)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View("Login", new Form
            {
                showSignInForm = true,
                showSignUpForm = false,
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
        public ActionResult SignUp()
        {
            return View("Login", new Form
            {
                showSignInForm = false,
                showSignUpForm = true,
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
}