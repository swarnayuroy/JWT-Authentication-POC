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
        private readonly IJwtManager _jwtManager;

        public AccountRepository(
            ILogger<AccountRepository> logger, 
            IService<User> userService, 
            IService<Account> accountService,
            IJwtManager jwtManager
        )
        {
            this._logger = new LoggerService<AccountRepository>(logger);
            this._userService = userService;
            this._accountService = accountService;
            this._jwtManager = jwtManager;
        }
        
        public async Task<ResponseDetail> CheckCredential(UserCredential userCredential)
        {
            // Get all users
            var users = await _userService.Get();
            // Find user by email
            _logger.LogDetails(LogType.INFO, $"getting user by email");
            var user = users.FirstOrDefault(u => u.Email.Equals(userCredential.Email, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                _logger.LogDetails(LogType.WARNING, "incorrect email");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Incorrect email"
                };
            }

            // Get all accounts
            var accounts = await _accountService.Get();
            
            // Find account by userId and verify password
            _logger.LogDetails(LogType.INFO, $"validating password for the user");
            var account = accounts.FirstOrDefault(a => a.UserId == user.Id && a.Password == userCredential.Password);
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
            // Get existing users to check for duplicate email
            var existingUsers = await _userService.Get();
            _logger.LogDetails(LogType.INFO, $"Checking if email exists");
            if (existingUsers.Any(u => u.Email.Equals(userRegistrationDetail.Email, StringComparison.OrdinalIgnoreCase)))
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
            var userSaved = await _userService.Save(newUser);

            if (!userSaved)
            {
                _logger.LogDetails(LogType.WARNING, $"Saving user process has been failed!");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Failed to create user"
                };
            }
            _logger.LogDetails(LogType.INFO, $"New user saved with id {newUser.Id} saved");

            // Create account for the user
            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                Password = userRegistrationDetail.Password,
                CreatedAt = DateTime.Now
            };

            // Save account
            var accountSaved = await _accountService.Save(newAccount);

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
            
            _logger.LogDetails(LogType.INFO, $"Account information saved for respective user, {newUser.Id}");            
            return new ResponseDetail
            {
                Status = true,
                Message = "Account created successfully"
            };
        }

        public async Task<ResponseDetail> DeleteAccount(string userId) {
            #region Find user and account details
            // Get all users
            var users = await _userService.Get();

            // Find user by Id
            _logger.LogDetails(LogType.INFO, $"getting user by id");
            var user = users.FirstOrDefault(u => u.Id.ToString() == userId);
            if (user == null)
            {
                _logger.LogDetails(LogType.WARNING, "Couldn't find user with the provided ID");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "User not found"
                };
            }

            // Get all accounts
            var accounts = await _accountService.Get();

            //Find account by userId
            _logger.LogDetails(LogType.INFO, $"getting account by userId");
            var account = accounts.FirstOrDefault(account=> account.UserId.ToString() == userId);
            if (account == null) 
            { 
                _logger.LogDetails(LogType.WARNING, "Couldn't find account for the user ID");
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Account details not found!"
                };
            }

            #endregion

            #region Proceed to delete user and account

            ResponseDetail response = new ResponseDetail();
            try
            {
                var isUserDeleted = await _userService.Delete(user.Id.ToString());
                var isAccountDeleted = await _accountService.Delete(account.Id.ToString());
                if (isUserDeleted && isAccountDeleted) {
                    _logger.LogDetails(LogType.INFO, $"User, {user.Name} has been deleted successfully.");
                    response = new ResponseDetail
                    {
                        Status = true,
                        Message = $"User, {user.Name} has been deleted successfully."
                    };                    
                }
                else
                {
                    throw new Exception($"Failed to delete user {user.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, $"{ex.Message}");
                await RollBackProcess(user, account, RollbackOperation.RETAIN);
                response = new ResponseDetail
                {
                    Status = true,
                    Message = $"Deletion of user, {user.Name} failed!"
                };
            }

            return response;

            #endregion
        }

        public async Task<ResponseDetail> EmailExists(string email)
        {
            // Get all users
            var users = await _userService.Get();
            // Find user by email
            _logger.LogDetails(LogType.INFO, $"getting user by email");
            var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
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
            string newOtp = await Task.Run(() => ProcessOtpService.GenerateOtp(user.Id, user.Email));

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
                // Get all users
                var users = await _userService.Get();
                // Find user by email
                _logger.LogDetails(LogType.INFO, $"getting user by email");
                var user = users.FirstOrDefault(u => u.Email.Equals(detail.Email, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {                    
                    user.IsVerified = true;
                    bool isUpdated = await _userService.Update(user);
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
            var users = await _userService.Get();
            // Find user by email
            _logger.LogDetails(LogType.INFO, $"getting user by email");
            var user = users.FirstOrDefault(u => u.Email.Equals(userCredential.Email, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                bool isSuccess = await Task.Run(() => ProcessOtpService.GetSuccessStatus(user.Id, user.Email));

                if (isSuccess)
                {
                    // Get all accounts
                    var accounts = await _accountService.Get();

                    // Find account by userId and verify password
                    _logger.LogDetails(LogType.INFO, $"fetching account detail...");
                    var account = accounts.FirstOrDefault(a => a.UserId == user.Id);

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
            bool isUserExists = _userService.Get().Result.Any(u => u.Id == userDetail.Id);
            bool isAccountExists = _accountService.Get().Result.Any(a => a.Id == accountDetail.Id);

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
                        await _userService.Delete(userDetail.Id.ToString());
                    }
                    if (isAccountExists)
                    {
                        await _accountService.Delete(accountDetail.Id.ToString());
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
