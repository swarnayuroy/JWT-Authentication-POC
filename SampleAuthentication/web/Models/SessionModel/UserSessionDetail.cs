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
        public bool? IsVerified { get; set; }
    }
    public class UserSessionDetail
    {
        public UserDetail User { get; set; }
        public ToastNotification ToastNotification { get; set; }
    }
}