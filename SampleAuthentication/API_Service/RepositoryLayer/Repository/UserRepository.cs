using API_Service.AppData;
using API_Service.Models.DTO;
using API_Service.Models.Entities;
using API_Service.Models.ResponseModel;
using API_Service.RepositoryLayer.Interface;
using API_Service.Utils;

namespace API_Service.RepositoryLayer.Repository
{
    public class UserRepository : IUserRepository
    {
        private LoggerService<UserRepository> _logger;
        private readonly IService<User> _userService;
        public UserRepository(ILogger<UserRepository> logger, IService<User> userService)
        {
            this._logger = new LoggerService<UserRepository>(logger);
            this._userService = userService;
        }
        public async Task<ResponseDetail> GetAllUsersAsync()
        {
            var users = await _userService.Get();
            if (users.Count() > 0)
            {
                _logger.LogDetails(LogType.INFO, $"Fetched {users.Count()} users");
                var userDetails = users.Select(user => new UserDetail
                {
                    Id = user.Id.ToString(),
                    Name = user.Name,
                    Email = user.Email,
                    IsVerified = user.IsVerified,
                    Password = string.Empty // Do not expose password
                });
                return new ResponseDataDetail<IEnumerable<UserDetail>>
                {
                    Status = true,
                    Message = userDetails.Count() > 1 ? $"{userDetails.Count()} users fetched successfully" : "1 user fetched successfully",
                    Data = userDetails
                };
            }
            return new ResponseDetail
            {
                Status = false,
                Message = "No users found"
            };
        }

        public async Task<ResponseDetail> GetUserAsync(string id)
        {
            var users = await _userService.Get();
            if (users.Count() > 0)
            {
                var user = users.FirstOrDefault(u => u.Id.ToString() == id);
                if (user != null)
                {
                    _logger.LogDetails(LogType.INFO, $"Successfully fetched user: {user.Id}");
                    return new ResponseDataDetail<UserDetail>
                    {
                        Status = true,
                        Message = "User fetched successfully",
                        Data = new UserDetail
                        {
                            Id = user.Id.ToString(),
                            Name = user.Name,
                            Email = user.Email,
                            IsVerified = user.IsVerified,
                            Password = string.Empty // Do not expose password
                        }
                    };
                }
            }
            return new ResponseDetail
            {
                Status = false,
                Message = "User not found"
            };
        }
    }
}
