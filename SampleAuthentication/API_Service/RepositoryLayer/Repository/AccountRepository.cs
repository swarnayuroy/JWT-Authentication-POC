using API_Service.AppData;
using API_Service.Utils;
using API_Service.Models.DTO;
using API_Service.Models.Entities;
using API_Service.Models.ResponseModel;
using API_Service.RepositoryLayer.Interface;

namespace API_Service.RepositoryLayer.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private LoggerService<AccountRepository> _logger;
        private readonly IService<User> _userService;
        private readonly IService<Account> _accountService;
        private readonly IUserDetailService _userDetailService;
        private readonly IAccountService _accountDataService;
        private readonly IJwtManager _jwtManager;

        public AccountRepository(
            ILogger<AccountRepository> logger, 
            IService<User> userService, 
            IService<Account> accountService,
            IUserDetailService userDetailService,
            IAccountService accountDataService,
            IJwtManager jwtManager
        )
        {
            this._logger = new LoggerService<AccountRepository>(logger);
            this._userService = userService;
            this._accountService = accountService;
            this._userDetailService = userDetailService;
            this._accountDataService = accountDataService;
            this._jwtManager = jwtManager;
        }
        
        public async Task<ResponseDetail> CheckCredential(UserCredential userCredential)
        {
            // Find user by email
            _logger.LogDetails(LogType.INFO, $"getting user by email");
            var user = await _userDetailService.GetUserByEmail(userCredential.Email);
            if (user == null)
            {
                _logger.LogDetails(LogType.WARNING, "incorrect email");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Incorrect email"
                };
            }

            // Find account by verifying the user id and password
            _logger.LogDetails(LogType.INFO, $"Validating password for the user...");
            var account = await _accountDataService.CheckAndGetAccount(user.Id.ToString(), userCredential.Password);
            
            if (account == null)
            {                
                _logger.LogDetails(LogType.WARNING, "incorrect password");
                return new ResponseDetail { Status = false, Message = "Incorrect password" };             
            }
            account.LoggedInAt = DateTime.Now;
            if (!await _accountService.Update(account))
            {
                _logger.LogDetails(LogType.ERROR, $"Failed to save login time for user {user.Id}");
                return new ResponseDetail { Status = false, Message = "Some error ocurred!" };
            }

            string userToken = _jwtManager.GenerateToken(user);
            if (!String.IsNullOrEmpty(userToken))
            {
                _logger.LogDetails(LogType.INFO, "Generated token successfully");
                return new ResponseDataDetail<string>
                {
                    Status = true,
                    Message = "Account validation successful",
                    Data = userToken
                };
            }
            _logger.LogDetails(LogType.WARNING, "Failed to generate token!");
            return new ResponseDetail 
            { 
                Status = false,
                Message = "Failed to generate token!"
            };
        }

        public async Task<ResponseDetail> RegisterUser(UserDetail userRegistrationDetail)
        {
            _logger.LogDetails(LogType.INFO, $"Checking if email exists");
            // Get existing users to check for duplicate email
            UserDetail existingUsers = await _userDetailService.GetUserByEmail(userRegistrationDetail.Email);            
            if (!string.IsNullOrEmpty(existingUsers.Email))
            {
                _logger.LogDetails(LogType.WARNING, $"The email is in use");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "User already exists!"
                };
            }

            // Create new user DTO
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Name = userRegistrationDetail.Name,
                Email = userRegistrationDetail.Email,
                Role = "User",
                IsVerified = false
            };
            // Save user
            bool userSaved = await _userService.Save(newUser);

            if (!userSaved)
            {
                _logger.LogDetails(LogType.WARNING, $"Saving user process has been failed!");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Failed to create user"
                };
            }
            _logger.LogDetails(LogType.INFO, $"New user, {newUser.Name} saved with id {newUser.Id}");

            // Create account for the user
            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                Password = userRegistrationDetail.Password,
                CreatedAt = DateTime.Now
            };

            // Save account
            bool accountSaved = await _accountService.Save(newAccount);

            // Rollback: If account save fails, delete the user that was just saved
            if (!accountSaved)
            {
                _logger.LogDetails(LogType.WARNING, $"Account saving process failed");
                await RollBackProcess(newUser, newAccount, RollbackOperation.REMOVE);

                return new ResponseDetail
                {
                    Status = false,
                    Message = "Failed to create account."
                };
            }
            
            _logger.LogDetails(LogType.INFO, $"Account information saved for respective user, {newUser.Name}");
            return new ResponseDetail
            {
                Status = true,
                Message = "Account created successfully"
            };
        }

        public async Task<ResponseDetail> DeleteAccount(string userId) {
            #region Find user and account details

            // Find user by Id
            _logger.LogDetails(LogType.INFO, $"getting user by id");
            var user = await _userDetailService.GetUser(userId);
            if (user == null)
            {
                _logger.LogDetails(LogType.WARNING, "Couldn't find user with the provided ID");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "User not found"
                };
            }

            //// Get all accounts
            //var accounts = await _accountService.Get();

            ////Find account by userId
            //_logger.LogDetails(LogType.INFO, $"getting account by userId");
            //var account = accounts.FirstOrDefault(account=> account.UserId.ToString() == userId);
            //if (account == null) 
            //{ 
            //    _logger.LogDetails(LogType.WARNING, "Couldn't find account for the user ID");
            //    return new ResponseDetail
            //    {
            //        Status = false,
            //        Message = "Account details not found!"
            //    };
            //}

            #endregion

            #region Proceed to delete user and account

            ResponseDetail response = new ResponseDetail();
            try
            {
                // Wait for both tasks to complete and collect results
                var isUserDeleted = await _userService.Delete(new User
                {
                    Id = Guid.Parse(userId),
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    IsVerified = user.IsVerified
                });

                if (isUserDeleted) {
                    _logger.LogDetails(LogType.INFO, $"User, {user.Name} has been deleted successfully.");
                    response = new ResponseDetail
                    {
                        Status = true,
                        Message = $"User, {user.Name} has been deleted successfully."
                    };                    
                }
                else
                {
                    _logger.LogDetails(LogType.INFO, $"Failed to delete user {user.Name}");
                    response = new ResponseDetail
                    {
                        Status = false,
                        Message = $"Failed to delete user {user.Name}!"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, $"{ex.Message}");
                //await RollBackProcess(user, account, RollbackOperation.RETAIN);
                response = new ResponseDetail
                {
                    Status = false,
                    Message = $"Some error occurred while deleting user {user.Name}!"
                };
            }

            return response;

            #endregion
        }

        public async Task<ResponseDetail> EmailExists(string email)
        {
            _logger.LogDetails(LogType.INFO, $"getting user by email");
            var user = await _userDetailService.GetUserByEmail(email);
            if (user == null)
            {
                _logger.LogDetails(LogType.WARNING, "incorrect email");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "This email does not exist!"
                };
            }

            _logger.LogDetails(LogType.INFO, $"Email exists, generating OTP...");
            string newOtp = await Task.Run(() => ProcessOtpService.GenerateOtp(Guid.Parse(user.Id), user.Email));

            if (!string.IsNullOrEmpty(newOtp))
            {
                _logger.LogDetails(LogType.INFO, $"OTP: {newOtp} generated successfully");
                return new ResponseDetail
                {
                    Status = true,
                    Message = "OTP has been sent to your email address"
                };
            }
            _logger.LogDetails(LogType.WARNING, $"Failed to generate OTP for email {user.Email}");
            return new ResponseDetail
            {
                Status = false,
                Message = "Some error occurred!"
            };
        }

        public async Task<ResponseDetail> Verify(VerifyAccount detail)
        {
            bool isVerified = await Task.Run(() => ProcessOtpService.ValidateOtp(detail.Email, detail.Otp));

            // logged in user needs to update their account verification status once verified
            if (isVerified && detail.IsLoggedIn)
            {
                _logger.LogDetails(LogType.INFO, $"OTP verification for email {detail.Email} is successful");

                // Find user by email
                _logger.LogDetails(LogType.INFO, $"getting user by email");
                var user = await _userDetailService.GetUserByEmail(detail.Email);
                if (user != null)
                {
                    bool isUpdated = await _userService.Update(new User
                    {
                        Id = Guid.Parse(user.Id),
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role,
                        IsVerified = true
                    });
                    await Task.Run(() => ProcessOtpService.ClearOtp(detail.Email));

                    if (isUpdated) 
                    {
                        _logger.LogDetails(LogType.INFO, $"Email: {detail.Email} has been verified successfully");                       

                        return new ResponseDetail
                        {
                            Status = true,
                            Message = "Account verified successfully"
                        };
                    }                    
                }
                
                _logger.LogDetails(LogType.WARNING, $"Failed to verify account for email: {detail.Email}");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Oops! some error ocurred."
                };
            }

            if (isVerified)
            {
                await Task.Run(() => ProcessOtpService.ClearOtp(detail.Email));
            }           
            
            _logger.LogDetails(LogType.INFO, $"OTP verification for email {detail.Email} is {(isVerified ? "successful" : "not successful")}");
            return new ResponseDetail
            {
                Status = isVerified,
                Message = isVerified ? "Account verified successfully" : "Invalid OTP"
            };
        }

        public async Task<ResponseDetail> SetPassword(UserCredential userCredential)
        {
            // Find user by email
            _logger.LogDetails(LogType.INFO, $"Getting user by email...");
            var user = await _userDetailService.GetUserByEmail(userCredential.Email);

            if (user != null)
            {
                bool isSuccess = await Task.Run(() => ProcessOtpService.GetSuccessStatus(Guid.Parse(user.Id), user.Email));

                if (isSuccess)
                {
                    // Find account by userId and verify password
                    _logger.LogDetails(LogType.INFO, $"Fetching account detail by user id and password...");
                    var account = await _accountDataService.CheckAndGetAccount(user.Id.ToString(), userCredential.Password);

                    if (account != null)
                    {
                        _logger.LogDetails(LogType.INFO, $"Successfully fetched account, setting new password for the account...");
                        account.Password = userCredential.Password;
                        bool isPasswordSet = await _accountService.Update(account);

                        if (isPasswordSet)
                        {
                            _logger.LogDetails(LogType.INFO, $"Password has been set successfully.");
                            await Task.Run(() => ProcessOtpService.ClearOtp(user.Email));

                            return new ResponseDetail
                            {
                                Status = true,
                                Message = "Password has been set successfully"
                            };
                        }
                    }
                    await Task.Run(() => ProcessOtpService.ClearOtp(user.Email));
                }

                _logger.LogDetails(LogType.WARNING, $"Denied to set password for email: {userCredential.Email}");
            }

            return new ResponseDetail
            {
                Status = false,
                Message = "Error occurred while setting password!"
            };
        }

        public async Task RollBackProcess(User userDetail, Account accountDetail, RollbackOperation operation)
        {
            var (isUserExists, isAccountExists) = await Task.WhenAll(
                _userDetailService.GetUser(userDetail.Id.ToString()).ContinueWith(task => task.Result != null),
                _accountDataService.GetAccountById(accountDetail.Id.ToString()).ContinueWith(task => task.Result != null)
            ).ContinueWith(task =>
            {
                var results = task.Result;
                return ((bool)results[0], (bool)results[1]);
            });
            switch (operation)
            {
                case RollbackOperation.RETAIN:
                    if (!isUserExists)
                    {
                        await _userService.Save(userDetail);
                    }
                    if (!isAccountExists)
                    {
                        await _accountService.Save(accountDetail);
                    }
                    _logger.LogDetails(LogType.INFO, $"Rollback: User with id {userDetail.Id} has been retained.");
                    break;
                case RollbackOperation.REMOVE:
                    if (isUserExists)
                    {
                        await _userService.Delete(userDetail);
                    }
                    if (isAccountExists)
                    {
                        await _accountService.Delete(accountDetail);
                    }
                    _logger.LogDetails(LogType.INFO, $"Rollback: User with id {userDetail.Id} has been removed.");
                    break;
                default:
                    _logger.LogDetails(LogType.WARNING, $"Rollback: Unknown operation '{operation}' specified."); 
                    break;
            }
        }
    }
}
