using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
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
    public class HttpServicePolicy
    {
        private Logger<HttpServicePolicy> _logger;
        public HttpServicePolicy()
        {
            _logger = new Logger<HttpServicePolicy>();
        }
        public AsyncPolicyWrap<HttpResponseMessage> BuildPolicy()
        {
            #region 1. TIMEOUT POLICY: 8 seconds per request
            
            // Triggers first to enforce per-request timeout control
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(10),
                TimeoutStrategy.Optimistic);
            #endregion

            #region 2. RETRY POLICY: 3 retries with exponential backoff (1s, 2s, 4s)
            
            // Retries on 5xx errors, timeouts, and transient HTTP exceptions
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r =>
                    (int)r.StatusCode >= 500 ||  // 5xx server errors
                    r.StatusCode == HttpStatusCode.RequestTimeout ||
                    r.StatusCode == HttpStatusCode.GatewayTimeout)  // Timeout responses
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),  // 2^n seconds
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var statusCode = outcome.Result?.StatusCode ?? HttpStatusCode.InternalServerError;
                        _logger.LogDetails(LogType.WARNING,
                            $"Retry attempt {retryCount} scheduled for {timespan.TotalSeconds}s. " +
                            $"Status: {statusCode} ({(int)statusCode})");
                    });
            #endregion

            #region 3. CIRCUIT BREAKER POLICY: Opens after 3 consecutive failures for 30 seconds
            
            // Prevents cascading failures by failing fast when service is degraded
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogDetails(LogType.ERROR,
                            $"Circuit breaker opened for {timespan.TotalSeconds}s. " +
                            $"Service is temporarily unavailable.");
                    },
                    onReset: () =>
                    {
                        _logger.LogDetails(LogType.INFO,
                            "Circuit breaker reset. Service recovered.");
                    });
            #endregion

            #region 4. FALLBACK POLICY: Returns ServiceUnavailable when all other policies fail
            // Provides graceful degradation instead of throwing exceptions
            var fallbackPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .FallbackAsync(
                    new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        ReasonPhrase = "Service temporarily unavailable (fallback)"
                    });
            #endregion
            
            //WRAP ALL POLICIES: Order matters (outer to inner)
            // Execution order: Fallback → Timeout → Retry → Circuit Breaker
            return Policy.WrapAsync(fallbackPolicy, timeoutPolicy, retryPolicy, circuitBreakerPolicy);
        }
    }
    
    public class HttpDataService : IHttpService
    {
        private Logger<HttpDataService> _logger;
        private readonly AsyncPolicyWrap<HttpResponseMessage> _policy;
        private readonly HttpClient _client;
        public HttpDataService() {
            this._logger = new Logger<HttpDataService>();
            this._policy = new HttpServicePolicy().BuildPolicy();
            this._client = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
            this._client.BaseAddress = new Uri(ConfigurationManager.AppSettings["serviceUri"]);
            this._client.DefaultRequestHeaders.Accept.Add
            (
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<HttpResponseMessage> CheckCredential(Credential credential, CancellationToken cancellationToken = default)
        {
            try
            {
                StringContent userCredential = new StringContent
                (
                    JsonConvert.SerializeObject(credential), 
                    Encoding.UTF8, "application/json"
                );
                _logger.LogDetails(LogType.INFO, "Checking user credential");
                
                HttpResponseMessage response = await _policy.ExecuteAsync(async cancelToken =>
                    await _client.PostAsync($"{_client.BaseAddress}/account/check", userCredential, cancelToken), 
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable (circuit breaker open)"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "CheckCredential request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "HTTP request failed"
                };
            }
        }

        public async Task<HttpResponseMessage> RegisterUser(Registration detail, CancellationToken cancellationToken = default)
        {
            try
            {
                StringContent userDetail = new StringContent
                (
                    JsonConvert.SerializeObject(detail), 
                    Encoding.UTF8, "application/json"
                );
                _logger.LogDetails(LogType.INFO, "Registering new user");
                
                HttpResponseMessage response = await _policy.ExecuteAsync(async cancelToken =>
                    await _client.PostAsync($"{_client.BaseAddress}/account/register", userDetail, cancelToken),
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "RegisterUser request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<HttpResponseMessage> CheckEmail(CheckEmail email, CancellationToken cancellationToken = default)
        {
            try
            {
                StringContent userEmail = new StringContent
                (
                    JsonConvert.SerializeObject(email),
                    Encoding.UTF8, "application/json"
                );
                _logger.LogDetails(LogType.INFO, $"Checking if email: {email} exists.");

                HttpResponseMessage response = await _policy.ExecuteAsync(async cancelToken =>
                    await _client.PostAsync($"{_client.BaseAddress}/account/emailexists", userEmail, cancelToken),
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "CheckEmail request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<HttpResponseMessage> VerifyAccount(VerifyAccount detail, CancellationToken cancellationToken = default)
        {
            try
            {
                StringContent verifyDetail = new StringContent
                (
                    JsonConvert.SerializeObject(detail),
                    Encoding.UTF8, "application/json"
                );
                _logger.LogDetails(LogType.INFO, "Verifying account in progress...");
                HttpResponseMessage response = await _policy.ExecuteAsync(async cancelToken =>
                    await _client.PostAsync($"{_client.BaseAddress}/account/verify", verifyDetail, cancelToken),
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "VerifyAccount request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<HttpResponseMessage> SetNewPassword(Credential credential, CancellationToken cancellationToken = default) 
        {
            try
            {
                StringContent credentialDetail = new StringContent
                (
                    JsonConvert.SerializeObject(credential),
                    Encoding.UTF8, "application/json"
                );
                _logger.LogDetails(LogType.INFO, $"Setting new password for email: {credential.Email}");
                
                HttpResponseMessage response = await _policy.ExecuteAsync(async cancelToken =>
                    await _client.PutAsync($"{_client.BaseAddress}/account/setpassword", credentialDetail, cancelToken),
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "SetNewPassword request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<HttpResponseMessage> GetUser(string token, string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_client.BaseAddress}/user/get/{userId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDetails(LogType.INFO, $"Fetching details for user: {userId}");

                HttpResponseMessage response = await _policy.ExecuteAsync(async cancelToken =>
                    await _client.SendAsync(request, cancelToken),
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable (circuit breaker open)"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "GetUser request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "HTTP request failed"
                };
            }
        }

        public async Task<HttpResponseMessage> GetUserDetail(string token, string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_client.BaseAddress}/user/getDetails/{userId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDetails(LogType.INFO, $"Fetching details for user: {userId}");
                
                HttpResponseMessage response = await _policy.ExecuteAsync(async cancelToken =>
                    await _client.SendAsync(request, cancelToken),
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "GetUserDetail request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<HttpResponseMessage> GetAllUsers(string token, string userId, string userType, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_client.BaseAddress}/user/get?userId={userId}&userType={userType}&page={page}&pageSize={pageSize}"
                );
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDetails(LogType.INFO, $"Getting all users");
                
                HttpResponseMessage response = await _policy.ExecuteAsync(
                    async cancelToken => await _client.SendAsync(request, cancelToken),
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "GetAllUsers request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<HttpResponseMessage> GetUsersBySearch(string token, string userId, int page, int pageSize, string searchText, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_client.BaseAddress}/user/get?userId={userId}&userType=User&page={page}&pageSize={pageSize}&searchText={searchText}"
                );
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDetails(LogType.INFO, $"Getting all users");
                
                HttpResponseMessage response = await _policy.ExecuteAsync(
                    async cancelToken => await _client.SendAsync(request, cancelToken),
                    cancellationToken
                );
                return response;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "GetUsersBySearch request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<HttpResponseMessage> DeleteAccount(string token, string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_client.BaseAddress}/account/delete/{userId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDetails(LogType.INFO, $"Deleting user: {userId}");
                
                HttpResponseMessage response = await _policy.ExecuteAsync(
                    async cancelToken => await _client.SendAsync(request, cancelToken),
                    cancellationToken
                );
                return response;

            }
            catch (BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, "Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, "DeleteAccount request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request cancelled"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDetails(LogType.ERROR, $"HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}