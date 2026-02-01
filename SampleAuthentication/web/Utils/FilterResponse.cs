using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using web.Models.ResponseModel;
using web.Repository;

namespace web.Utils
{
    public class FilterResponse<T> where T : class
    {
        private Logger<T> _logger;
        public FilterResponse()
        {
           this._logger = new Logger<T>();
        }
        public async Task<ResponseDetail> Process(HttpResponseMessage response)
        {
            if (response.Content != null)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogDetails(LogType.INFO, $"API Response Content: {responseContent}");

                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    try
                    {
                        // Try to parse as JSON object
                        var jsonResponse = JObject.Parse(responseContent);
                        // Try both "Message" and "message" (case-insensitive)
                        string message = jsonResponse["Message"]?.ToString() ?? jsonResponse["message"]?.ToString();

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            return new ResponseDetail
                            {
                                Status = response.IsSuccessStatusCode,
                                StatusCode = response.StatusCode,
                                Message = message
                            };
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        _logger.LogDetails(LogType.WARNING, $"Failed to parse JSON: {ex.Message}");

                        // If it's not JSON, treat it as plain string
                        string message = responseContent.Trim().Trim('"');

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            return new ResponseDetail
                            {
                                Status = response.IsSuccessStatusCode,
                                StatusCode = response.StatusCode,
                                Message = message
                            };
                        }
                    }
                }
            }

            // Fallback to ReasonPhrase
            return new ResponseDetail
            {
                Status = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Message = response.ReasonPhrase ?? "Failed to process the request"
            };
        }
    }
}