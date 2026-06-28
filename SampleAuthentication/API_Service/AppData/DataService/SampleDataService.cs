using API_Service.Models.DTO;
using API_Service.Models.Entities;
using API_Service.Utils;
using DataContext.DataProvider;
using DataContext.DataService;
using DataContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace API_Service.AppData.DataService
{
    public class Data
    {
        private IEnumerable<UserDetail> _userDetails = Enumerable.Empty<UserDetail>();
        private Models.Entities.Account _usersAccount = new Models.Entities.Account();
        private UserDetail _userDetail = new UserDetail();
        private FullUserDetail _fullUserDetail = new FullUserDetail();

        public IEnumerable<UserDetail> Users
        {
            get
            {
                return _userDetails;
            }

            set
            {
                if (value.Any())
                {
                    _userDetails = value;
                }
            }
        }

        public UserDetail UserDetail
        {
            get { return _userDetail; }
            set
            {
                if (value != null)
                {
                    _userDetail = value;
                }
            }
        }

        public FullUserDetail FullUserDetail 
        {
            get { return _fullUserDetail; }
            set
            {
                if (value != null)
                {
                    _fullUserDetail = value;
                }
            }
        }

        public Models.Entities.Account AccountDetail
        {
            get
            {
                return _usersAccount;
            }

            set
            {
                if (value!=null)
                {
                    _usersAccount = value;
                }
            }
        }
    }
    public class SampleDataService<T> : IService<T> where T : class
    {
        private LoggerService<SampleDataService<T>> _logger;
        //private readonly IDataProvider _dataProvider;
        //private readonly IDataService _dataService;
        private readonly IContextProvider _dataContextProvider;
        private readonly IContextService _dataContextService;
        private readonly IUnitOfWork _taskExecution;

        public SampleDataService(
            ILogger<SampleDataService<T>> logger, 
            /*IDataProvider dataProvider, IDataService dataService,*/                       // Inmemory data provider and service for testing
            IContextProvider dataContextProvider, IContextService dataContextService,   // Database context provider and service for production
            IUnitOfWork taskExecution
        )
        {
            this._logger = new LoggerService<SampleDataService<T>>(logger);
            //this._dataProvider = dataProvider;
            //this._dataService = dataService;
            this._dataContextProvider = dataContextProvider;
            this._dataContextService = dataContextService;
            this._taskExecution = taskExecution;
        }

        public Task<IEnumerable<T>> Get()
        {
            var dataContext = new Data();
            //if (typeof(T) == typeof(Models.Entities.User))
            //{
            //    var users = (from user in _dataContextProvider.User
            //                 join userRole in _dataContextProvider.UserRole
            //                 on user.Id equals userRole.UserId
            //                 select new Models.Entities.User
            //                 {
            //                     Id = user.Id, // Fix: assign Guid directly, not string
            //                     Name = user.Name,
            //                     Email = user.Email,
            //                     Role = userRole.Role == UserRoleType.Superadmin ? "Superadmin" : userRole.Role == UserRoleType.Admin ? "Admin" : "User",
            //                     IsVerified = user.IsVerified
            //                 }).ToList<Models.Entities.User>();

            //    _logger.LogDetails(LogType.INFO, $"Fetched {users.Count()} users from data provider.");

            //    dataContext.User = users;                
            //    return Task.FromResult((IEnumerable<T>)dataContext.User!);
            //}
            //else if (typeof(T) == typeof(Models.Entities.Account))
            //{
            //    var accountDetails = _dataContextProvider.Account.Select(a => new Models.Entities.Account
            //    {
            //        Id = a.Id,
            //        UserId = a.UserId,
            //        Password = a.Password,
            //        CreatedAt = a.CreatedAt,
            //        LoggedInAt = a.LoggedInAt
            //    });
            //    _logger.LogDetails(LogType.INFO, $"Fetched {accountDetails.Count()} accounts from data provider.");

            //    dataContext.AccountDetail = accountDetails;
            //    return Task.FromResult((IEnumerable<T>)dataContext.AccountDetail!);
            //}

            _logger.LogDetails(LogType.WARNING, $"Type {typeof(T).Name} is not supported type");
            throw new NotFiniteNumberException($"Type {typeof(T).Name} is not supported type");
        }

        public async Task<bool> Save(T entity)
        {
            try
            {
                if (typeof(T) == typeof(Models.Entities.User))
                {
                    var userDetail = entity as Models.Entities.User;
                    if (userDetail == null)
                    {
                        return false;
                    }

                    // Convert DTO to Domain Model
                    var user = new DataContext.Models.User
                    {
                        Id = userDetail.Id,
                        Name = userDetail.Name,
                        Email = userDetail.Email,                        
                        IsVerified = userDetail.IsVerified
                    };
                    
                    var userRole = new UserRole
                    {
                        UserId = userDetail.Id,
                        Role = userDetail.Role.Equals("Superadmin") ? UserRoleType.Superadmin : userDetail.Role.Equals("Admin") ? UserRoleType.Admin : UserRoleType.User
                    };

                    if (await _taskExecution.ExecuteAndCommit(
                        () => _dataContextService.SaveUserAsync(user),
                        () => _dataContextService.SaveUserRoleAsync(userRole)
                    ))
                    {
                        _logger.LogDetails(LogType.INFO, $"User: {user.Id} saved successfully.");
                        return true;
                    }
                    else
                    {
                        throw new Exception($"Failed to save user: {user.Id}");
                    }
                }
                else if (typeof(T) == typeof(Models.Entities.Account))
                {
                    var accountDetail = entity as Models.Entities.Account;
                    if (accountDetail == null)
                    {
                        return false;
                    }

                    // Convert DTO to Domain Model
                    var account = new DataContext.Models.Account
                    {
                        Id = accountDetail.Id,
                        UserId = accountDetail.UserId,
                        Password = accountDetail.Password,
                        CreatedAt = accountDetail.CreatedAt,
                        LoggedInAt = accountDetail.LoggedInAt
                    };

                    if (await _taskExecution.ExecuteAndCommit(
                        () => _dataContextService.SaveAccountAsync(account)
                    ))
                    {
                        _logger.LogDetails(LogType.INFO, $"Account: {account.Id} saved successfully.");
                        return true;
                    }
                    else
                    {
                        throw new Exception($"Failed to save account: {account.Id}");
                    }
                    
                }

                _logger.LogDetails(LogType.WARNING, $"Type {typeof(T).Name} is not supported type");
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported type");
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, $"{ex.Message}");
                return false;
            }
        }

        public async Task<bool> Update(T entity) 
        {
            try
            {
                if (typeof(T) == typeof(Models.Entities.User))
                {
                    var userDetail = entity as Models.Entities.User;
                    if (userDetail != null)
                    {
                        return await _taskExecution.ExecuteAndCommit(() => _dataContextService.UpdateUserAsync(
                            new DataContext.Models.User
                            {
                                Id = userDetail.Id,
                                Name = userDetail.Name,
                                Email = userDetail.Email,
                                IsVerified = userDetail.IsVerified
                            }
                        ));
                    }
                }
                else if (typeof(T) == typeof(Models.Entities.Account))
                {
                    var accountDetail = entity as Models.Entities.Account;
                    if (accountDetail != null)
                    {
                        return await _taskExecution.ExecuteAndCommit(() => _dataContextService.UpdateAccountAsync(
                            new DataContext.Models.Account
                            {
                                Id = accountDetail.Id,
                                UserId = accountDetail.UserId,
                                Password = accountDetail.Password,
                                CreatedAt = accountDetail.CreatedAt,
                                LoggedInAt = accountDetail.LoggedInAt
                            }
                        ));
                    }
                }
                _logger.LogDetails(LogType.WARNING, $"Type {typeof(T).Name} is not supported type");
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported type");
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, $"{ex.Message}");
                return false;
            }            
        }

        public async Task<bool> Delete(T entity)
        {
            try
            {
                if (typeof(T) == typeof(Models.Entities.User))
                {
                    var userDetail = entity as Models.Entities.User;
                    if (userDetail != null)
                    {
                        return await _taskExecution.ExecuteAndCommit(() => 
                                        _dataContextService.DeleteUserAsync(new DataContext.Models.User
                                        {
                                            Id = userDetail.Id,
                                            Name = userDetail.Name,
                                            Email = userDetail.Email,
                                            IsVerified = userDetail.IsVerified
                                        })
                                     );
                    }
                }
                //else if (typeof(T) == typeof(Models.Entities.Account))  // this operation isn't valid while working with DbContext - as cascading behavior has been set.
                //{
                //    var accountDetail = entity as Models.Entities.Account;
                //    if (accountDetail != null)
                //    {
                //        return await _taskExecution.ExecuteAndCommit(() => 
                //                        _dataService.DeleteAccountAsync(accountDetail.Id) // Assuming DeleteAccountAsync takes an ID for deletion
                //                     );
                //    }
                //}

                _logger.LogDetails(LogType.WARNING, $"Type {typeof(T).Name} is not supported type");
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported type");
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, $"{ex.Message}");
                return Convert.ToBoolean(await Task.FromResult(false));
            }
        }
    }

    public class UserDataService: IUserDetailService
    {
        private LoggerService<UserDataService> _logger;
        private readonly IContextProvider _dataContextProvider;
        public UserDataService(ILogger<UserDataService> logger, IContextProvider dataContextProvider)
        {
            _logger = new LoggerService<UserDataService>(logger);
            _dataContextProvider = dataContextProvider;
        }
        public async Task<UserDetail> GetUser(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var dataContext = new Data();
                var userDetail = await (from user in _dataContextProvider.User
                                  join role in _dataContextProvider.UserRole on user.Id equals role.UserId
                                  where user.Id.ToString() == id
                                  select new UserDetail
                                  {
                                      Id = user.Id.ToString(),
                                      Name = user.Name,
                                      Email = user.Email,
                                      Role = role.Role == UserRoleType.Superadmin ? "Superadmin" : role.Role == UserRoleType.Admin ? "Admin" : "User",
                                      IsVerified = user.IsVerified,
                                      Password = string.Empty
                                  }).FirstOrDefaultAsync();

                if (userDetail != null)
                {
                    _logger.LogDetails(LogType.INFO, $"User found with id {id}");
                    dataContext.UserDetail = userDetail;
                    return dataContext.UserDetail;
                }
            }
            
            _logger.LogDetails(LogType.WARNING, $"Couldn't find user with id {id}");
            return new UserDetail();
        }

        public async Task<FullUserDetail> GetUserDetail(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var dataContext = new Data();
                var userDetail = await (from user in _dataContextProvider.User
                                  join role in _dataContextProvider.UserRole on user.Id equals role.UserId
                                  join account in _dataContextProvider.Account on user.Id equals account.UserId
                                  where user.Id.ToString() == id
                                  select new FullUserDetail
                                  {
                                      Id = user.Id.ToString(),
                                      Name = user.Name,
                                      Email = user.Email,
                                      Role = role.Role == UserRoleType.Superadmin ? "Superadmin" : role.Role == UserRoleType.Admin ? "Admin" : "User",
                                      IsVerified = user.IsVerified,
                                      LoggedInAt = account.LoggedInAt == null ? DateTime.MinValue : account.LoggedInAt,
                                      AccountOld = Convert.ToString(account.CreatedAt)
                                  }).FirstOrDefaultAsync();

                if (userDetail != null)
                {
                    _logger.LogDetails(LogType.INFO, $"Details found for user id {id}");

                    dataContext.FullUserDetail = userDetail;
                    return dataContext.FullUserDetail;
                }
            }
            _logger.LogDetails(LogType.WARNING, $"Couldn't find details for user {id}");
            return new FullUserDetail();
        }

        public async Task<UserDetail> GetUserByEmail(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var dataContext = new Data();
                var userDetail = await (from user in _dataContextProvider.User
                                  join role in _dataContextProvider.UserRole on user.Id equals role.UserId
                                  where user.Email == email
                                  select new UserDetail
                                  {
                                      Id = user.Id.ToString(),
                                      Name = user.Name,
                                      Email = user.Email,
                                      Role = role.Role == UserRoleType.Superadmin ? "Superadmin" : role.Role == UserRoleType.Admin ? "Admin" : "User",
                                      IsVerified = user.IsVerified,
                                      Password = string.Empty
                                  }).FirstOrDefaultAsync();

                if (userDetail != null)
                {
                    _logger.LogDetails(LogType.INFO, $"User found with email {email}");
                    dataContext.UserDetail = userDetail;
                    return dataContext.UserDetail;
                }
            }
            _logger.LogDetails(LogType.WARNING, $"Couldn't find user with email {email}");
            return new UserDetail();
        }

        public async Task<IEnumerable<UserDetail>> GetUsersByType(string userType)
        {
            if (!string.IsNullOrEmpty(userType))
            {
                var dataContext = new Data();
                switch (userType)
                {
                    case "Admin":
                        var adminUsers = await (from user in _dataContextProvider.User
                                          join role in _dataContextProvider.UserRole on user.Id equals role.UserId
                                          where role.Role == UserRoleType.Superadmin || role.Role == UserRoleType.Admin
                                          select new UserDetail
                                          {
                                              Id = user.Id.ToString(),
                                              Name = user.Name,
                                              Email = user.Email,
                                              Role = role.Role == UserRoleType.Superadmin ? "Superadmin" : "Admin",
                                              IsVerified = user.IsVerified,
                                              Password = string.Empty
                                          }).ToListAsync();

                        if (adminUsers.Count() > 0)
                        {
                            if (adminUsers.Count() == 1)
                            {
                                _logger.LogDetails(LogType.INFO, $"Fetched 1 admin only.");
                            }
                            else
                            {
                                _logger.LogDetails(LogType.INFO, $"Fetched {adminUsers.Count()} admins.");
                            }
                            dataContext.Users = adminUsers;
                            return dataContext.Users;
                        }
                        _logger.LogDetails(LogType.WARNING, $"No admin users found!");
                        return Enumerable.Empty<UserDetail>();

                    case "User":
                        var users = await (from user in _dataContextProvider.User
                                     join role in _dataContextProvider.UserRole on user.Id equals role.UserId
                                     where role.Role == UserRoleType.User
                                     select new UserDetail
                                     {
                                         Id = user.Id.ToString(),
                                         Name = user.Name,
                                         Email = user.Email,
                                         Role = "User",
                                         IsVerified = user.IsVerified,
                                         Password = string.Empty
                                     }).ToListAsync();

                        if (users.Count() > 0)
                        {
                            if (users.Count() == 1)
                            {
                                _logger.LogDetails(LogType.INFO, $"Fetched 1 user only.");
                            }
                            else
                            {
                                _logger.LogDetails(LogType.INFO, $"Fetched {users.Count()} users.");
                            }
                            dataContext.Users = users;
                            return dataContext.Users;
                        }
                        _logger.LogDetails(LogType.WARNING, $"No users found!");
                        return Enumerable.Empty<UserDetail>();

                    default:
                        _logger.LogDetails(LogType.WARNING, $"No users found of type {userType}");
                        break;
                }
            }
            _logger.LogDetails(LogType.WARNING, $"Couldn't generate result for empty type");
            return Enumerable.Empty<UserDetail>();
        }

        public async Task<IEnumerable<UserDetail>> FindUsers(string searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var dataContext = new Data();
                var userResults = await (from user in _dataContextProvider.User
                                   join userRole in _dataContextProvider.UserRole on user.Id equals userRole.UserId
                                   where userRole.Role == UserRoleType.User &&
                                   (
                                    EF.Functions.Like(user.Name, $"%{searchTerm}%") ||
                                    EF.Functions.Like(user.Email, $"%{searchTerm}%")
                                   )
                                   select new UserDetail
                                   {
                                       Id = user.Id.ToString(),
                                       Name = user.Name,
                                       Email = user.Email,
                                       Role = "User",
                                       IsVerified = user.IsVerified,
                                       Password = string.Empty
                                   }).ToListAsync();

                if (userResults.Count() > 0)
                {
                    if (userResults.Count() == 1)
                    {
                        _logger.LogDetails(LogType.INFO, $"Fetched 1 user only for the search term: {searchTerm}.");
                    }
                    else
                    {
                        _logger.LogDetails(LogType.INFO, $"Fetched {userResults.Count()} users for the search term: {searchTerm}.");
                    }
                    dataContext.Users = userResults;
                    return dataContext.Users;
                }
                _logger.LogDetails(LogType.WARNING, $"No users found for search term: {searchTerm}");
                return Enumerable.Empty<UserDetail>();
            }
            _logger.LogDetails(LogType.WARNING, $"Couldn't generate result for empty search term");
            return Enumerable.Empty<UserDetail>();
        }
    }

    public class AccountDataService : IAccountService
    {
        private LoggerService<AccountDataService> _logger;
        private readonly IContextProvider _dataContextProvider;
        public AccountDataService(ILogger<AccountDataService> logger, IContextProvider dataContextProvider)
        {
            this._logger = new LoggerService<AccountDataService>(logger);
            this._dataContextProvider = dataContextProvider;
        }
        public async Task<Models.Entities.Account> CheckAndGetAccount(string userId, string password)
        {
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
            {
                var dataContext = new Data();
                if (Guid.TryParse(userId, out Guid id))
                {
                    var accountDetail = await (from account in _dataContextProvider.Account 
                                         where account.UserId == id && account.Password == password
                                         select new Models.Entities.Account
                                         {
                                             Id = account.Id,
                                             UserId = account.UserId,
                                             CreatedAt = account.CreatedAt,
                                             Password = account.Password
                                         }).FirstOrDefaultAsync();

                    if (accountDetail != null)
                    {
                        _logger.LogDetails(LogType.INFO, $"Account found for UserId: {userId}");
                        dataContext.AccountDetail = accountDetail;
                        return dataContext.AccountDetail;
                    }
                }
            }
            _logger.LogDetails(LogType.WARNING, $"Couldn't find account for UserId: {userId} with the provided password.");
            return new Models.Entities.Account();
        }

        public async Task<Models.Entities.Account> GetAccountById(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (Guid.TryParse(id, out Guid accountId))
                {
                    var dataContext = new Data();
                    var accountDetail = await (from account in _dataContextProvider.Account
                                         where account.Id == accountId
                                         select new Models.Entities.Account
                                         {
                                            Id= account.Id,
                                            UserId = account.UserId,
                                            CreatedAt = account.CreatedAt,
                                            Password = account.Password,
                                         }).FirstOrDefaultAsync();

                    if (accountDetail != null)
                    {
                        _logger.LogDetails(LogType.INFO, $"Account found");
                        dataContext.AccountDetail = accountDetail;
                        return dataContext.AccountDetail;
                    }
                }                
            }
            _logger.LogDetails(LogType.WARNING, $"Couldn't find any account");
            return new Models.Entities.Account();
        }
    }
}
