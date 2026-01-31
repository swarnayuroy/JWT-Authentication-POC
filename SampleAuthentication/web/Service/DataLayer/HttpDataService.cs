using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using web.Models;

namespace web.Service.DataLayer
{
    public class HttpDataService : IHttpService
    {
        private readonly HttpClient _client;
        public HttpDataService() {
            this._client = new HttpClient();
            this._client.BaseAddress = new Uri(ConfigurationManager.AppSettings["serviceUri"]);
            this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task<HttpResponseMessage> CheckCredential(Credential credential)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> RegisterUser(Registration detail)
        {
            throw new NotImplementedException();
        }
    }
}