using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using web.Models.ResponseModel;
using web.Models.SessionModel;
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

                        // Extract properties (case-insensitive)
                        string message = jsonResponse["Message"]?.ToString() ?? jsonResponse["message"]?.ToString();
                        bool? statusFromJson = jsonResponse["Status"]?.ToObject<bool?>() ?? jsonResponse["status"]?.ToObject<bool?>();
                        bool status = statusFromJson ?? response.IsSuccessStatusCode;

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            // No Data property found, return standard ResponseDetail
                            return new ResponseDetail
                            {
                                Status = status,
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
                    catch (Exception ex)
                    {
                        _logger.LogDetails(LogType.ERROR, $"Error processing response: {ex.Message}");
                        return new ResponseDetail
                        {
                            Status = false,
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            Message = response.ReasonPhrase ?? "Failed to process the request"
                        };
                    }
                }
            }

            // Fallback to ReasonPhrase
            return new ResponseDetail
            {
                Status = false,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = response.ReasonPhrase ?? "Failed to process the request"
            };
        }
        public async Task<ResponseDetail> ProcessData<TData>(HttpResponseMessage response)
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

                        // Extract properties (case-insensitive)
                        string message = jsonResponse["Message"]?.ToString() ?? jsonResponse["message"]?.ToString();
                        bool? statusFromJson = jsonResponse["Status"]?.ToObject<bool?>() ?? jsonResponse["status"]?.ToObject<bool?>();
                        bool status = statusFromJson ?? response.IsSuccessStatusCode;

                        // Check if Data property exists
                        var data = jsonResponse["Data"] ?? jsonResponse["data"];

                        if (data != null)
                        {
                            // Response has Data property - try to determine the type
                            // For now, we'll assume string type for token scenarios
                            // This can be extended for other types if needed
                            try
                            {
                                if (typeof(TData).Equals(typeof(string)))
                                {
                                    string responseData = data.ToObject<string>();

                                    return new ResponseDataDetail<string>
                                    {
                                        Status = status,
                                        StatusCode = response.StatusCode,
                                        Message = message ?? string.Empty,
                                        Data = responseData
                                    };
                                }
                                else if (typeof(TData).Equals(typeof(UserDetail)))
                                {
                                    UserDetail userData = data.ToObject<UserDetail>();
                                    if (userData != null)
                                    {
                                        return new ResponseDataDetail<UserDetail>
                                        {
                                            Status = status,
                                            StatusCode = response.StatusCode,
                                            Message = message ?? string.Empty,
                                            Data = userData
                                        };
                                    }
                                }
                            }
                            catch
                            {
                                // If it's not a string, treat as object and log
                                _logger.LogDetails(LogType.INFO, "Could not determine the type of the Data received.");

                                return new ResponseDetail
                                {
                                    Status = status,
                                    StatusCode = response.StatusCode,
                                    Message = message ?? string.Empty
                                };
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(message))
                        {
                            // No Data property found, return standard ResponseDetail
                            return new ResponseDetail
                            {
                                Status = status,
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
                    catch (Exception ex)
                    {
                        _logger.LogDetails(LogType.ERROR, $"Error processing response: {ex.Message}");
                        return new ResponseDetail
                        {
                            Status = false,
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            Message = response.ReasonPhrase ?? "Failed to process the request"
                        };
                    }
                }
            }
            // Fallback to ReasonPhrase
            return new ResponseDetail
            {
                Status = false,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = response.ReasonPhrase ?? "Failed to process the request"
            };
        }
    }
}