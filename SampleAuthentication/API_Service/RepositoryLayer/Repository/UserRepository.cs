using API_Service.AppData;
using API_Service.Models.DTO;
using API_Service.Models.Entities;
using API_Service.RepositoryLayer.Interface;

namespace API_Service.RepositoryLayer.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IService<User> _userService;
        public UserRepository(IService<User> userService)
        {
            this._userService = userService;
        }
        public async Task<IEnumerable<UserDetail>> GetAllUsersAsync()
        {
            var users = await _userService.Get();
            if (users.Count()>0)
            {
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
