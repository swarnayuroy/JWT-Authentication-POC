using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Service.DTO
{
    public sealed class UserDetail
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
    }
}