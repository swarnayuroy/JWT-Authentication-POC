using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using web.Models;

namespace web.Service
{
    public interface IHttpService
    {
        Task<HttpResponseMessage> CheckCredential(Credential credential, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> RegisterUser(Registration detail, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> CheckEmail(CheckEmail email, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> VerifyAccount(VerifyAccount detail, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> SetNewPassword(Credential credential, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> GetUser(string token, string userId, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> GetUserDetail(string token, string userId, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> GetAllUsers(string token, string userId, string userType, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> GetUsersBySearch(string token, string userId, int page, int pageSize, string searchText, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeleteAccount(string token, string userId, CancellationToken cancellationToken = default);
    }
}
