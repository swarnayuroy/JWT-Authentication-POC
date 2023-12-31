﻿using AuthenticationJWT.Models;
using AuthenticationJWT.Utils;
using AutoMapper;
using log4net;
using ServiceLayer.DTO;
using ServiceLayer.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AuthenticationJWT.Controllers
{
    public class LoginController : Controller
    {
        #region Declaration and Initialization
        private readonly ILog _logger;
        private readonly IService _service;
        private readonly IMap _mapper;
        public LoginController(IService service, IMap mapper)
        {
            _service = service;
            _mapper = mapper;
            _logger = LogManager.GetLogger(typeof(LoginController));
        }
        #endregion

        #region Login
        [HttpGet]
        public async Task<ActionResult> SignIn()
        {
            Form usrForm = null;
            try
            {                
                await Task.Run(()=>usrForm = new Form());
                return View(usrForm);
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}\n{ex.StackTrace}");
                return View("Some error occurred!");
            }            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignIn(Form credential)
        {            
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("SignIn", credential);
                }
                
                string usrName = $"";
                User usrCred = _mapper.GetUserCredential(credential.Login);
                UserDTO user = _mapper.GetUserDTO(usrCred);

                string token = await Task.Run(()=> _service.ConfirmValidCredential(user));
                if (!string.IsNullOrEmpty(token))
                {
                    HttpCookie cookie = new HttpCookie("userToken", token);
                    cookie.HttpOnly = true;
                    Response.Cookies.Add(cookie);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}\n{ex.StackTrace}");
                ModelState.Clear();
                ViewBag.Error = $"We encountered some problem. Please try again later.";
                return View("SignIn");
            }
            ViewBag.InvalidCred = $"Invalid credential!";
            return View("SignIn", credential);
        }
        #endregion

        #region Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(Form newUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("SignIn", newUser);
                }

                UserDTO user = _mapper.GetUserDTO(newUser.Register);
                bool isRegistered = await Task.Run(()=>_service.RegisterNewUser(user));
                if (isRegistered)
                {
                    ModelState.Clear();
                    return View("SignIn");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}\n{ex.StackTrace}");
                ModelState.Clear();
                ViewBag.Error = $"We encountered some problem. Please try again later.";
                return View("SignIn");
            }
            ModelState.Clear();
            ViewBag.Error = $"Registration failed! Please try again later.";            
            return View("SignIn");
        }

        public async Task<JsonResult> IsEmailInUse(Form newUser)
        {
            try
            {
                if (newUser.Register != null)
                {
                    bool isEmailExist = await Task.Run(() => _service.IsEmailExist(newUser.Register.Email));
                    return Json(!isEmailExist, JsonRequestBehavior.AllowGet);
                }                
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}\n{ex.StackTrace}");                
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}