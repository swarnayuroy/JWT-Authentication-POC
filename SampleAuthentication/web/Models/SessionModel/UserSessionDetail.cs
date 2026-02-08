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
    }
    public class UserSessionDetail
    {
        private string _avatarText;
        public string AvatarText
        {
            get
            {
                return _avatarText.ToUpper();
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    string[] nameArray = value.Split(' ');
                    _avatarText = nameArray.Length > 1 ? $"{nameArray[0][0]}{nameArray[nameArray.Length - 1][0]}" : $"{nameArray[0][0]}";
                }
                else
                {
                    _avatarText = "?";
                }
            }
        }
        public UserDetail User { get; set; }
        public ToastNotification ToastNotification { get; set; }
    }
}