using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using web.Models;
using System.Threading.Tasks;

namespace web.Service
{
    public interface IHttpService
    {
        Task<HttpResponseMessage> CheckCredential(Credential credential);
        Task<HttpResponseMessage> RegisterUser(Registration detail);
        Task<HttpResponseMessage> CheckEmail(CheckEmail email);
        Task<HttpResponseMessage> VerifyAccount(VerifyAccount detail);
        Task<HttpResponseMessage> SetNewPassword(Credential credential);
        Task<HttpResponseMessage> GetUser(string token, string userId);
        Task<HttpResponseMessage> GetUserDetail(string token, string userId);
        Task<HttpResponseMessage> GetAllUsers(string token, string userId, string userType, int page, int pageSize);
        Task<HttpResponseMessage> GetUsersBySearch(string token, string userId, int page, int pageSize, string searchText);
    }
}
