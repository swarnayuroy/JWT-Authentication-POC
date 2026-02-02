using API_Service.AppData;
using API_Service.Utils;
using API_Service.Models.DTO;
using API_Service.Models.Entities;
using API_Service.RepositoryLayer.Interface;

namespace API_Service.RepositoryLayer.Repository
{
    public class UserRepository : IUserRepository
    {
        private LoggerService<UserRepository> _logger;
        private readonly IService<User> _userService;
        public UserRepository(IService<User> userService)
        {
            this._logger = new LoggerService<UserRepository>(new LoggerFactory().CreateLogger<UserRepository>());
            this._userService = userService;
        }
        public async Task<IEnumerable<UserDetail>> GetAllUsersAsync()
        {
            var users = await _userService.Get();
            
            if (users.Count()>0)
            {
                _logger.LogDetails(LogType.INFO, $"Fetched {users.Count()} users");
                var userDetails = users.Select(user => new UserDetail
                {
                    Id = user.Id.ToString(),
                    Name = user.Name,
                    Email = user.Email,
                    Password = string.Empty // Do not expose password
                }); 
                return userDetails;
            }
            return Enumerable.Empty<UserDetail>();
        }
        public async Task<UserDetail?> GetUserAsync(string id)
        {
            var users = await _userService.Get();
            if (users.Count() > 0)
            {
                var user = users.FirstOrDefault(u => u.Id.ToString() == id);
                if (user != null)
                {
                    _logger.LogDetails(LogType.INFO, $"Successfull fetch for {user.Id}");
                    return new UserDetail
                    {
                        Id = user.Id.ToString(),
                        Name = user.Name,
                        Email = user.Email,
                        Password = string.Empty // Do not expose password
                    };
                }
            }
            return null;
        }
    }
}
