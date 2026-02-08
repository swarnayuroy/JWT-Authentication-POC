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

        public AccountRepository(
            ILogger<AccountRepository> logger, 
            IService<User> userService, 
            IService<Account> accountService
        )
        {
            this._logger = new LoggerService<AccountRepository>(logger);
            this._userService = userService;
            this._accountService = accountService;
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

            string userToken = new JwtManager().GenerateToken(user);
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
                await RollBackUser(newUser.Id.ToString());

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
        public async Task RollBackUser(string userId)
        {            
            await _userService.Delete(userId);
            _logger.LogDetails(LogType.INFO, $"Rollback: User with id {userId} has been deleted.");
        }
    }
}
