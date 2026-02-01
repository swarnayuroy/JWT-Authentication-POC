using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using web.Models;
using web.Utils;

namespace web.Service.DataLayer
{
    public class HttpDataService : IHttpService
    {
        private Logger<HttpDataService> _logger;
        private readonly HttpClient _client;
        public HttpDataService() {
            this._logger = new Logger<HttpDataService>();
            this._client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            this._client.BaseAddress = new Uri(ConfigurationManager.AppSettings["serviceUri"]);
            this._client.DefaultRequestHeaders.Accept.Add
            (
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<HttpResponseMessage> CheckCredential(Credential credential)
        {
            try
            {
                StringContent userCredential = new StringContent
                (
                    JsonConvert.SerializeObject(credential), 
                    Encoding.UTF8, "application/json"
                );
                _logger.LogDetails(LogType.INFO, "Checking user credential");
                HttpResponseMessage response = await _client.PostAsync
                (
                    $"{_client.BaseAddress}/account/check", 
                    userCredential
                );
                return response;
            }
            catch (TaskCanceledException ex)
            {
                if (ex.InnerException is TimeoutException)
                {
                    return new HttpResponseMessage(HttpStatusCode.RequestTimeout) { 
                        ReasonPhrase = "Request timeout" 
                    };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError) { 
                        ReasonPhrase = "Request cancelled" 
                    };
                }
            }
        }

        public async Task<HttpResponseMessage> RegisterUser(Registration detail)
        {
            try
            {
                StringContent userDetail = new StringContent
                (
                    JsonConvert.SerializeObject(detail), 
                    Encoding.UTF8, "application/json"
                );
                _logger.LogDetails(LogType.INFO, "Registering new user");
                HttpResponseMessage response = await _client.PostAsync
                (
                    $"{_client.BaseAddress}/account/register", 
                    userDetail
                );
                return response;    
            }
            catch (TaskCanceledException ex)
            {
                if (ex.InnerException is TimeoutException)
                {
                    return new HttpResponseMessage(HttpStatusCode.RequestTimeout) 
                    { 
                        ReasonPhrase = "Request timeout" 
                    };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError) 
                    { 
                        ReasonPhrase = "Request cancelled" 
                    };
                }
            }
        }
    }
}