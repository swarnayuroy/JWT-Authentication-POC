using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace web.Models.SessionModel
{
    public class VerifyUser
    {
        public bool showOTP_Field { get; set; } = false;
        public VerifyOTP OTP_Field { get; set; }
        public string CloseVerifyModal { get { return "fa-solid fa-circle-xmark"; } }
    }
    public class UserDetail 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAdmin { get { return Role.Equals("Superadmin") || Role.Equals("Admin") ? true : false; } }
    }
    public class FullUserDetail : UserDetail
    {
        public DateTime LoggedInAt { get; set; }
        public string AccountOld { get; set; }
    }
    public class UserSessionDetail
    {
        public string ViewText { get { return "Razor"; } }
        public UserDetail User { get; set; }
        public VerifyUser Verify { get; set; } = new VerifyUser();
        public ToastNotification ToastNotification { get; set; }
    }
    
    public class AdminSessionDetail : UserSessionDetail
    {
        public AdminType AdminType { get { return User.Role.Equals("Superadmin") ? AdminType.Superadmin : AdminType.Admin; } }
        public SessionData Data { get; set; } = null;
    }
    public class SessionData
    {
        public int UserCount { get; set; }
        public IEnumerable<UserDetail> Users { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalPages { get; set; }
    }
    public enum AdminType
    {
        Admin,
        Superadmin,
    }
}