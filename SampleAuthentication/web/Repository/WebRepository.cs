using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using web.Models;
using web.Models.ResponseModel;
using web.Service;
using web.Service.DTO;

namespace web.Repository
{
    public class WebRepository : IWebRepository
    {
        private readonly IService<UserDetail> _userService;
        private readonly IService<UserAccountDetail> _accountService;

        public WebRepository(IService<UserDetail> userService, IService<UserAccountDetail> accountService)
        {
            _userService = userService;
            _accountService = accountService;
        }

        public async Task<ResponseDetail> CheckCredential(Credential userCredential)
        {
            // Get all users
            var users = await _userService.Get();
            
            // Find user by email
            var user = users.FirstOrDefault(u => u.Email.Equals(userCredential.Email, StringComparison.OrdinalIgnoreCase));
            
            if (user == null)
            {
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Incorrect email!"
                };
            }

            // Get all accounts
            var accounts = await _accountService.Get();
            
            // Find account by userId and verify password
            var account = accounts.FirstOrDefault(a => a.UserId.ToString() == user.Id && a.Password == userCredential.Password);

            return account != null ? new ResponseDetail { Status = true } : new ResponseDetail { Status = false, Message = "Incorrect password"};
        }
        public async Task<ResponseDetail> RegisterUser(Registration userRegistrationDetail)
        {
            // Get existing users to check for duplicate email
            var existingUsers = await _userService.Get();
            
            if (existingUsers.Any(u => u.Email.Equals(userRegistrationDetail.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return new ResponseDetail
                {
                    Status = false,
                    Message = "User already exists!"
                };
            }

            // Create new user DTO
            var newUser = new UserDetail
            {
                Id = Guid.NewGuid().ToString(),
                Name = userRegistrationDetail.Name,
                Email = userRegistrationDetail.Email,
                IsVerified = false
            };

            // Save user
            var userSaved = await _userService.Save(newUser);
            
            if (!userSaved)
            {
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Failed to create user"
                };
            }

            // Create account for the user
            var newAccount = new UserAccountDetail
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(newUser.Id),
                Password = userRegistrationDetail.Password,
                CreatedAt = DateTime.Now,
                LoggedInAt = DateTime.MinValue
            };

            // Save account
            var accountSaved = await _accountService.Save(newAccount);
            
            // Rollback: If account save fails, delete the user that was just saved
            if (!accountSaved)
            {
                await RollBackUser(newUser.Id);
                
                return new ResponseDetail
                {
                    Status = false,
                    Message = "Failed to create account."
                };
            }
            
            
            return new ResponseDetail
            {
                Status = true,
                Message = "Account created successfully"
            };
        }
        public async Task RollBackUser(string userId) {
            await _userService.Delete(userId);
        }
    }
}