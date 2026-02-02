using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using web.Models;
using web.Models.ResponseModel;
using web.Service;
using web.Service.DTO;
using web.Utils;

namespace web.Repository
{
    public class WebRepository : IWebRepository
    {
        private Logger<WebRepository> _logger;
        private readonly IHttpService _httpService;

        private HttpResponseMessage _response;

        public WebRepository(IHttpService httpService)
        {
            this._logger = new Logger<WebRepository>();
            this._httpService = httpService;
        }

        public async Task<ResponseDetail> CheckCredential(Credential userCredential)
        {
            #region HTTTP Service Call

            try
            {
                _response = await _httpService.CheckCredential(userCredential);

                if (_response.IsSuccessStatusCode)
                {
                    return new ResponseDetail
                    {
                        Status = true,
                        StatusCode = _response.StatusCode,
                        Message = "Credential verified successfully"
                    };
                }

                return await new FilterResponse<WebRepository>().Process(_response);
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, $"Exception in CheckCredential: {ex.Message}");

                return _response?.Content != null ?
                    new ResponseDetail { Status = false, StatusCode = _response.StatusCode, Message = $"Invalid response." }
                    : new ResponseDetail { Status = false, StatusCode = HttpStatusCode.BadRequest, Message = $"Failed to process your request" };
            }

            #endregion
        }
        public async Task<ResponseDetail> RegisterUser(Registration userRegistrationDetail)
        {
            #region HTTP Service Call

            try
            {
                _response = await _httpService.RegisterUser(userRegistrationDetail);

                return await new FilterResponse<WebRepository>().Process(_response);
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, ex.Message);

                return _response?.Content != null ?
                    new ResponseDetail { Status = false, StatusCode = _response.StatusCode, Message = $"Invalid response." }
                    : new ResponseDetail { Status = false, StatusCode = HttpStatusCode.BadRequest, Message = $"Failed to process your request" };
            }

            #endregion
        }
    }
}