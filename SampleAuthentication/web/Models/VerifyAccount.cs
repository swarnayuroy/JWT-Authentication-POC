using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Models
{
    public class VerifyAccount
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}