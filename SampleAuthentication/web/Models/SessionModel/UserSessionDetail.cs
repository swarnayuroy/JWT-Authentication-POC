using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Models.SessionModel
{
    public class UserDetail 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool? IsVerified { get; set; }
        public bool IsAdmin { get { return Role.Equals("Admin") ? true : false; } }
    }
    public class UserSessionDetail
    {
        public string ViewText { get { return "Razor"; } }
        public UserDetail User { get; set; }
        public ToastNotification ToastNotification { get; set; }
    }
    public class AdminSessionDetail : UserSessionDetail
    {
        public SessionData Data { get; set; } = null;
    }
    public class SessionData
    {
        public int UserCount { get{ return Users.Count(); } }
        public IEnumerable<UserDetail> Users { get; set; }
    }
}