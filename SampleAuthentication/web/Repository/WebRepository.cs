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

        private readonly IService<UserDetail> _userService;
        private readonly IService<UserAccountDetail> _accountService;

        private HttpResponseMessage _response;

        public WebRepository
        (
            IHttpService httpService,
            IService<UserDetail> userService,
            IService<UserAccountDetail> accountService
        )
        {
            this._logger = new Logger<WebRepository>();
            this._httpService = httpService;
            this._userService = userService;
            this._accountService = accountService;
        }

        public async Task<ResponseDetail> CheckCredential(Credential userCredential)
        {
            #region working with SampleData

            //// Get all users
            //var users = await _userService.Get();

            //// Find user by email
            //var user = users.FirstOrDefault(u => u.Email.Equals(userCredential.Email, StringComparison.OrdinalIgnoreCase));

            //if (user == null)
            //{
            //    return new ResponseDetail
            //    {
            //        Status = false,
            //        Message = "Incorrect email!"
            //    };
            //}

            //// Get all accounts
            //var accounts = await _accountService.Get();

            //// Find account by userId and verify password
            //var account = accounts.FirstOrDefault(a => a.UserId.ToString() == user.Id && a.Password == userCredential.Password);

            //return account != null ? new ResponseDetail { Status = true } : new ResponseDetail { Status = false, Message = "Incorrect password"};

            #endregion

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

                // Handle error response
                string responseContent = null;

                if (_response.Content != null)
                {
                    responseContent = await _response.Content.ReadAsStringAsync();
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
                                    Status = false,
                                    StatusCode = _response.StatusCode,
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
                                    Status = false,
                                    StatusCode = _response.StatusCode,
                                    Message = message
                                };
                            }
                        }
                    }
                }

                // Fallback to ReasonPhrase
                return new ResponseDetail
                {
                    Status = false,
                    StatusCode = _response.StatusCode,
                    Message = _response.ReasonPhrase ?? "Request failed"
                };
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
            #region working with SampleData

            //// Get existing users to check for duplicate email
            //var existingUsers = await _userService.Get();

            //if (existingUsers.Any(u => u.Email.Equals(userRegistrationDetail.Email, StringComparison.OrdinalIgnoreCase)))
            //{
            //    return new ResponseDetail
            //    {
            //        Status = false,
            //        Message = "User already exists!"
            //    };
            //}

            //// Create new user DTO
            //var newUser = new UserDetail
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = userRegistrationDetail.Name,
            //    Email = userRegistrationDetail.Email,
            //    IsVerified = false
            //};

            //// Save user
            //var userSaved = await _userService.Save(newUser);

            //if (!userSaved)
            //{
            //    return new ResponseDetail
            //    {
            //        Status = false,
            //        Message = "Failed to create user"
            //    };
            //}

            //// Create account for the user
            //var newAccount = new UserAccountDetail
            //{
            //    Id = Guid.NewGuid(),
            //    UserId = Guid.Parse(newUser.Id),
            //    Password = userRegistrationDetail.Password,
            //    CreatedAt = DateTime.Now,
            //    LoggedInAt = DateTime.MinValue
            //};

            //// Save account
            //var accountSaved = await _accountService.Save(newAccount);

            //// Rollback: If account save fails, delete the user that was just saved
            //if (!accountSaved)
            //{
            //    await RollBackUser(newUser.Id);

            //    return new ResponseDetail
            //    {
            //        Status = false,
            //        Message = "Failed to create account."
            //    };
            //}            

            //return new ResponseDetail
            //{
            //    Status = true,
            //    Message = "Account created successfully"
            //};

            #endregion

            #region HTTP Service Call

            try
            {
                _response = await _httpService.RegisterUser(userRegistrationDetail);

                if (_response.Content != null)
                {
                    string responseContent = await _response.Content.ReadAsStringAsync();

                    try
                    {
                        // Try to parse as JSON object
                        var jsonResponse = JObject.Parse(responseContent);
                        // Try both "Message" and "message" (case-insensitive)
                        string message = jsonResponse["Message"]?.ToString() ?? jsonResponse["message"]?.ToString();

                        return new ResponseDetail
                        {
                            Status = _response.IsSuccessStatusCode,
                            StatusCode = _response.StatusCode,
                            Message = message ?? _response.ReasonPhrase
                        };
                    }
                    catch (JsonReaderException)
                    {
                        // If it's not JSON (plain string from Created response)
                        string message = responseContent.Trim('"');
                        return new ResponseDetail
                        {
                            Status = _response.IsSuccessStatusCode,
                            StatusCode = _response.StatusCode,
                            Message = message
                        };
                    }
                }

                return new ResponseDetail
                {
                    Status = _response.IsSuccessStatusCode,
                    StatusCode = _response.StatusCode,
                    Message = _response.ReasonPhrase
                };
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
        public async Task RollBackUser(string userId)
        {
            await _userService.Delete(userId);
        }
    }
}