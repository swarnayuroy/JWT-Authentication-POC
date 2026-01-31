using System;
using System.Net;
using System.Web;

namespace web.Models
{
    public class ToastNotification
    {
        public static readonly string SUCCESS_ICON = "fa-solid fa-circle-check";
        public static readonly string WARNING_ICON = "fa fa-triangle-exclamation";
        public bool IsEnable { get; set; }
        public HttpStatusCode Type { get; set; }
        public string StatusIcon { get; set; }
        public string Message { get; set; }
        public int TimeOut { get { return 8000; } }
        public string CloseToastIcon { get { return "fa-solid fa-circle-xmark"; } }
    }

    public class Form
    {
        public bool showSignInForm { get; set; }
        public bool showSignUpForm { get; set; }
        public Credential SignIn { get; set; }
        public Registration SignUp { get; set; }
        public ToastNotification ToastNotification { get; set; }
    }
}