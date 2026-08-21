using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            #region 1. TIMEOUT POLICY

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(12),
                TimeoutStrategy.Optimistic);

            #endregion

            #region 2. RETRY POLICY

            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r =>
                    (int)r.StatusCode >= 500 ||
                    r.StatusCode == HttpStatusCode.RequestTimeout ||
                    r.StatusCode == HttpStatusCode.GatewayTimeout)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var statusCode =
                            outcome.Result?.StatusCode ??
                            HttpStatusCode.InternalServerError;

                        _logger.LogDetails(
                            LogType.WARNING,
                            $"Retry attempt {retryCount} scheduled for " +
                            $"{timespan.TotalSeconds}s. " +
                            $"Status: {statusCode} ({(int)statusCode})");
                    });

            #endregion

            #region 3. CIRCUIT BREAKER POLICY

            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r =>
                    (int)r.StatusCode >= 500)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogDetails(
                            LogType.ERROR,
                            $"Circuit breaker opened for " +
                            $"{timespan.TotalSeconds}s. " +
                            $"Service is temporarily unavailable.");
                    },
                    onReset: () =>
                    {
                        _logger.LogDetails(
                            LogType.INFO,
                            "Circuit breaker reset. Service recovered.");
                    });

            #endregion

            #region 4. FALLBACK POLICY

            var fallbackPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r =>
                    (int)r.StatusCode >= 500)
                .FallbackAsync(
                    new HttpResponseMessage(
                        HttpStatusCode.ServiceUnavailable)
                    {
                        ReasonPhrase =
                            "Service temporarily unavailable (fallback)"
                    });

            #endregion

            // Execution order:
            // Fallback -> Retry -> Timeout -> Circuit Breaker

            return Policy.WrapAsync(
                fallbackPolicy,
                retryPolicy,
                timeoutPolicy,
                circuitBreakerPolicy);
        }
    }


    public class HttpDataService : IHttpService
    {
        private readonly Logger<HttpDataService> _logger;
        private readonly AsyncPolicyWrap<HttpResponseMessage> _policy;
        private readonly HttpClient _client;

        private readonly SemaphoreSlim _bulkhead;
        private const int bulkheadLimit = 10;
        private const int bulkheadQueueLimit = 20;
        private int _queuedRequests;

        public HttpDataService()
        {
            _logger = new Logger<HttpDataService>();
            _policy = new HttpServicePolicy().BuildPolicy();
            _client = new HttpClient{
                Timeout = TimeSpan.FromSeconds(12),
                BaseAddress = new Uri(ConfigurationManager.AppSettings["serviceUri"])
            };
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            _bulkhead = new SemaphoreSlim(bulkheadLimit, bulkheadLimit);
        }

        #region Bulkhead Control and Execution

        private async Task<bool> EnterBulkheadAsync(CancellationToken cancellationToken)
        {
            if (!_bulkhead.Wait(0))
            {
                // No immediate slot.
                // Check whether we can join the queue.
                var queued = Interlocked.Increment(ref _queuedRequests);

                if (queued > bulkheadQueueLimit)
                {
                    Interlocked.Decrement(ref _queuedRequests);
                    return false;
                }
                try
                {
                    await _bulkhead.WaitAsync(cancellationToken);
                    return true;
                }
                finally
                {
                    Interlocked.Decrement(ref _queuedRequests);
                }                
            }
            return true;
        }

        private void ExitBulkhead()
        {
            _bulkhead.Release();
        }

        private async Task<HttpResponseMessage> ExecuteWithBulkheadAsync(Func<CancellationToken, Task<HttpResponseMessage>> action, CancellationToken cancellationToken)
        {
            if (await EnterBulkheadAsync(cancellationToken))
            {
                try
                {
                    return await action(cancellationToken);
                }
                finally
                {
                    ExitBulkhead();
                }
            }

            _logger.LogDetails(LogType.WARNING, "Bulkhead limit reached. Request rejected.");

            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                ReasonPhrase = "Service is busy. Bulkhead limit reached."
            };
        }
        
        #endregion

        #region HTTP Helpers

        private StringContent CreateJsonContent<T>(T data)
        {
            return new StringContent(
                JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json"
            );
        }

        private Uri CreateUri(string relativeUrl)
        {
            return new Uri(_client.BaseAddress, relativeUrl);
        }

        private async Task<HttpResponseMessage> SendAsync(
            Func<HttpRequestMessage> requestFactory,
            CancellationToken cancellationToken)
        {
            return await ExecuteWithBulkheadAsync( async cancelToken => {
                return await _policy.ExecuteAsync(async policyToken => {
                    using (var request = requestFactory()) {
                        return await _client.SendAsync(request, policyToken);
                    }
                }, cancelToken);
            }, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendJsonAsync<T>(
            HttpMethod method,
            string relativeUrl,
            T data,
            CancellationToken cancellationToken)
        {
            return await ExecuteWithBulkheadAsync(async cancelToken => {
                return await _policy.ExecuteAsync(async policyToken => {
                    using (var request = new HttpRequestMessage(method, CreateUri(relativeUrl)))
                    {
                        request.Content = CreateJsonContent(data);
                        return await _client.SendAsync(request, policyToken);
                    }
                }, cancelToken);
            }, cancellationToken);
        }

        private HttpResponseMessage HandleException(
            Exception ex,
            string operation)
        {
            if (ex is BrokenCircuitException)
            {
                _logger.LogDetails(LogType.ERROR, $"{operation}: Circuit breaker is open. Service unavailable.");
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable){
                    ReasonPhrase = "Service temporarily unavailable"
                };
            }

            if (ex is OperationCanceledException)
            {
                _logger.LogDetails(LogType.WARNING, $"{operation}: Request was cancelled.");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout){
                    ReasonPhrase = "Request cancelled"
                };
            }

            if (ex is HttpRequestException)
            {
                _logger.LogDetails(LogType.ERROR, $"{operation}: HTTP request failed: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError){
                    ReasonPhrase = "HTTP request failed"
                };
            }

            if (ex is TimeoutRejectedException) 
            {
                _logger.LogDetails(LogType.WARNING, $"{operation}: Encountered request timed out: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request timed out"
                };
            }

            _logger.LogDetails(LogType.ERROR, $"{operation}: Unexpected error: {ex.Message}");
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Unexpected error"
            };
        }

        #endregion

        #region Public API

        public async Task<HttpResponseMessage> CheckCredential(
            Credential credential,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, "Checking user credential");
                return await SendJsonAsync(HttpMethod.Post, "account/check", credential, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(CheckCredential));
            }
        }


        public async Task<HttpResponseMessage> RegisterUser(
            Registration detail,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, "Registering new user");
                return await SendJsonAsync(HttpMethod.Post, "account/register", detail, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(RegisterUser));
            }
        }


        public async Task<HttpResponseMessage> CheckEmail(
            CheckEmail email,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, $"Checking if email: {email} exists.");
                return await SendJsonAsync(HttpMethod.Post, "account/emailexists", email, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(CheckEmail));
            }
        }


        public async Task<HttpResponseMessage> VerifyAccount(
            VerifyAccount detail,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, "Verifying account in progress...");
                return await SendJsonAsync(HttpMethod.Post, "account/verify", detail, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(VerifyAccount));
            }
        }


        public async Task<HttpResponseMessage> SetNewPassword(
            Credential credential,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, $"Setting new password for email: {credential.Email}");
                return await SendJsonAsync(HttpMethod.Put, "account/setpassword", credential, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(SetNewPassword));
            }
        }


        public async Task<HttpResponseMessage> GetUser(
            string token,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, $"Fetching details for user: {userId}");
                return await SendAsync(() => {
                    var request = new HttpRequestMessage(HttpMethod.Get, CreateUri($"user/get/{userId}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    return request;
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(GetUser));
            }
        }


        public async Task<HttpResponseMessage> GetUserDetail(
            string token,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, $"Fetching details for user: {userId}");

                return await SendAsync(() => {
                    var request = new HttpRequestMessage(HttpMethod.Get, CreateUri($"user/getDetails/{userId}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                    return request;
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(GetUserDetail));
            }
        }


        public async Task<HttpResponseMessage> GetAllUsers(
            string token,
            string userId,
            string userType,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, "Getting all users");

                return await SendAsync(() => {
                    var url = $"user/get?userId={userId}&userType={userType}&page={page}&pageSize={pageSize}";

                    var request = new HttpRequestMessage(HttpMethod.Get, CreateUri(url));
                    request.Headers.Authorization =  new AuthenticationHeaderValue("Bearer", token);

                    return request;
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(
                    ex,
                    nameof(GetAllUsers));
            }
        }


        public async Task<HttpResponseMessage> GetUsersBySearch(
            string token,
            string userId,
            int page,
            int pageSize,
            string searchText,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, "Getting users by search");

                return await SendAsync(() => {
                    var url = $"user/get?userId={userId}&userType=User&page={page}&pageSize={pageSize}&searchText={searchText}";

                    var request = new HttpRequestMessage(HttpMethod.Get, CreateUri(url));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    return request;
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(GetUsersBySearch));
            }
        }


        public async Task<HttpResponseMessage> DeleteAccount(
            string token,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDetails(LogType.INFO, $"Deleting user: {userId}");

                return await SendAsync(() => {
                    var request = new HttpRequestMessage(HttpMethod.Delete, CreateUri($"account/delete/{userId}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    return request;
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(DeleteAccount));
            }
        }

        #endregion
    }
}