using API_Service.Models.DTO;
using API_Service.Models.Entities;
using DataContext.DataProvider;
using DataContext.DataService;
using DataContext.Models;
using System.Threading.Tasks;

namespace API_Service.AppData.DataService
{
    public class Data
    {
        private IEnumerable<Models.Entities.User> _user = Enumerable.Empty<Models.Entities.User>();
        private IEnumerable<Models.Entities.Account> _usersAccount = Enumerable.Empty<Models.Entities.Account>();

        public IEnumerable<Models.Entities.User> User
        {
            get
            {
                return _user;
            }

            set
            {
                if (value.Count() > 0)
                {
                    _user = value;
                }
            }
        }
        public IEnumerable<Models.Entities.Account> AccountDetail
        {
            get
            {
                return _usersAccount;
            }

            set
            {
                if (value.Count() > 0)
                {
                    _usersAccount = value;
                }
            }
        }
    }
    public class SampleDataService<T> : IService<T> where T : class
    {
        private readonly IDataProvider _dataProvider;
        private readonly IDataService _dataService;
        public SampleDataService(IDataProvider dataProvider, IDataService dataService)
        {
            this._dataProvider = dataProvider;
            this._dataService = dataService;
        }

        public Task<IEnumerable<T>> Get()
        {
            var dataContext = new Data();
            if (typeof(T) == typeof(Models.Entities.User))
            {
                var users = _dataProvider.User.Select(u => new Models.Entities.User
                {
                    Id = u.Id, // Fix: assign Guid directly, not string
                    Name = u.Name,
                    Email = u.Email,
                    IsVerified = u.IsVerfied
                });
                dataContext.User = users;
                return Task.FromResult((IEnumerable<T>)dataContext.User!);
            }
            else if (typeof(T) == typeof(Models.Entities.Account))
            {
                var accountDetails = _dataProvider.Account.Select(a => new Models.Entities.Account
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Password = a.Password,
                    CreatedAt = a.CreatedAt,
                    LoggedInAt = a.LoggedInAt
                });
                dataContext.AccountDetail = accountDetails;
                return Task.FromResult((IEnumerable<T>)dataContext.AccountDetail!);
            }

            throw new NotSupportedException($"Type {typeof(T).Name} is not supported type");
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
                        IsVerfied = userDetail.IsVerified
                    };

                    await _dataService.SaveUserAsync(user);
                    return true;
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

                    await _dataService.SaveAccountAsync(account);
                    return true;
                }

                throw new NotSupportedException($"Type {typeof(T).Name} is not supported type");
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<bool> Delete(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid guidId))
                {
                    return Task.FromResult(false);
                }

                if (typeof(T) == typeof(Models.Entities.User))
                {
                    return _dataService.DeleteUserAsync(guidId);
                }
                else if (typeof(T) == typeof(Models.Entities.Account))
                {
                    return _dataService.DeleteAccountAsync(guidId);
                }

                throw new NotSupportedException($"Type {typeof(T).Name} is not supported type");
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}
