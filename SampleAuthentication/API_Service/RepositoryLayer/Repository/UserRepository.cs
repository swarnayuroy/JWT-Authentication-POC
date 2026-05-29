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
        private readonly IUserDetailService _userDetailService;
        public UserRepository(ILogger<UserRepository> logger, IService<User> userService, IUserDetailService userDetailService)
        {
            this._logger = new LoggerService<UserRepository>(logger);
            this._userService = userService;
            this._userDetailService = userDetailService;
        }
        public async Task<ResponseDetail> GetAllUsersAsync(string userId, string userType, int page, int pageSize)
        {
            var users = await _userService.Get();
            if (users.Any())
            {
                IEnumerable<User> filteredUser = new List<User>();
                if (userType == "Admin") 
                {
                    filteredUser = users.Where(user => user.Role == "Superadmin" || user.Role == "Admin");
                }
                else
                {
                    filteredUser = users.Where(user => user.Role == "User");
                }               
                
                int totalUserCount = filteredUser.Count();
                if (totalUserCount > 0)
                {
                    var paginatedUsers = filteredUser
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .Select(user => new UserDetail
                                     {
                                         Id = user.Id.ToString(),
                                         Name = user.Name,
                                         Email = user.Email,
                                         Role = user.Role,
                                         IsVerified = user.IsVerified
                                     });
                    _logger.LogDetails(LogType.INFO, $"Fetched page {page} ({paginatedUsers.Count()} of {totalUserCount} users)");

                    return new ResponseDataDetail<PagedResult<UserDetail>>
                    {
                        Status = true,
                        Message = totalUserCount > 1 ? $"{totalUserCount} users fetched successfully" : "1 user fetched successfully",
                        Data = new PagedResult<UserDetail>
                        {
                            Items = paginatedUsers,
                            ItemCount = totalUserCount,
                            CurrentPage = page,
                            PageSize = pageSize
                        }
                    };
                }                
            }
            return new ResponseDetail
            {
                Status = false,
                Message = "No users found"
            };
        }
        public async Task<ResponseDetail> GetUserBySearch(string userId, int page, int pageSize, string searchText)
        {
            var users = await _userService.Get();
            if (users.Any())
            {
                _logger.LogDetails(LogType.INFO, $"Searching users with term '{searchText}'");
                var filteredUsers = users
                                    .Where(user => user.Role == "User")
                                    .Where(user => string.IsNullOrEmpty(searchText) ||
                                                   user.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                                   user.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase));

                int totalUserCount = filteredUsers.Count();
                if (totalUserCount > 0)
                {
                    var paginatedUsers = filteredUsers
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .Select(user => new UserDetail
                                     {
                                         Id = user.Id.ToString(),
                                         Name = user.Name,
                                         Email = user.Email,
                                         Role = user.Role,
                                         IsVerified = user.IsVerified
                                     });
                    _logger.LogDetails(LogType.INFO, $"Fetched page {page} ({paginatedUsers.Count()} of {totalUserCount} users) for search term '{searchText}'");
                    return new ResponseDataDetail<PagedResult<UserDetail>>
                    {
                        Status = true,
                        Message = totalUserCount > 1 ? $"{totalUserCount} users fetched successfully" : "1 user fetched successfully",
                        Data = new PagedResult<UserDetail>
                        {
                            Items = paginatedUsers,
                            ItemCount = totalUserCount,
                            CurrentPage = page,
                            PageSize = pageSize
                        }
                    };
                }
            }
            return new ResponseDetail
            {
                Status = false,
                Message = $"No users found matching with search text '{searchText}'"
            };
        }
        public async Task<ResponseDetail> GetUserAsync(string id)
        {
            var user = await _userDetailService.GetUser(id);
            if (user != null)
            {
                _logger.LogDetails(LogType.INFO, $"Successfully fetched user: {user.Id}");
                return new ResponseDataDetail<UserDetail>
                {
                    Status = true,
                    Message = "User fetched successfully",
                    Data = user
                };
            }            
            return new ResponseDetail
            {
                Status = false,
                Message = "User not found"
            };
        }
        public async Task<ResponseDetail> GetUserDetailAsync(string id)
        {
            var userDetail = await _userDetailService.GetUserDetail(id);
            if (userDetail != null)
            {
                _logger.LogDetails(LogType.INFO, $"Successfully fetched user detail: {userDetail.Id}");
                return new ResponseDataDetail<FullUserDetail>
                {
                    Status = true,
                    Message = "User detail fetched successfully",
                    Data = userDetail
                };
            }
            return new ResponseDetail
            {
                Status = false,
                Message = "User detail not found"
            };
        }
    }
}
