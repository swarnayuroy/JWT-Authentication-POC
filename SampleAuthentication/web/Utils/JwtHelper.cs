using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Web;

namespace web.Utils
{
    public class JwtHelper
    {
        public static ClaimsPrincipal DecodeToken(string sessionToken)
        {
            if (!String.IsNullOrEmpty(sessionToken))
            {
                var token = new JwtSecurityTokenHandler().ReadJwtToken(sessionToken);
                var identity = new ClaimsIdentity(token.Claims);
                return new ClaimsPrincipal(identity);
            }
            return null;
        }
    }
}