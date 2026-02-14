using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API_Service.Models.Entities;

namespace API_Service.Utils
{
    public interface IJwtManager
    {
        string GenerateToken(User userDetail);
    }
    public class JwtManager : IJwtManager
    {
        private readonly IConfiguration _configuration;
        public JwtManager(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        public string GenerateToken(User userDetail)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userDetail.Id.ToString()),
                    new Claim(ClaimTypes.Name, userDetail.Name),
                    new Claim(ClaimTypes.Email, userDetail.Email)
                }),
                Issuer = _configuration["JwtConfig:Issuer"],
                Audience = _configuration["JwtConfig:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtConfig:TokenValidityMins")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey
                                        (Encoding.ASCII.GetBytes(_configuration["JwtConfig:Key"])), 
                                        SecurityAlgorithms.HmacSha256Signature
                                     )
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
    }
}
