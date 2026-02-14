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
        Task<HttpResponseMessage> GetUserDetail(string token, string userId);
    }
}
