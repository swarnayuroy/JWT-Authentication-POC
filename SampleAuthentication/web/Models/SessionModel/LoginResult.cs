using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Models.SessionModel
{
    public class LoginResult
    {
        public string Type { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Handoff { get; set; } = string.Empty;
    }
}