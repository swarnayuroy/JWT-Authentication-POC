using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Service.DTO
{
    public sealed class UserAccountDetail
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LoggedInAt { get; set; }
    }
}