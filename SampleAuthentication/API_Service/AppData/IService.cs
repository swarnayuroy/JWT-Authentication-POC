using API_Service.Models.Entities;
using API_Service.Models.DTO;

namespace API_Service.AppData
{
    public interface IService<T> where T : class
    {
        Task<IEnumerable<T>> Get();
        Task<bool> Save(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(T entity);
    }
    public interface IUserDetailService
    {
        Task<UserDetail> GetUser(string id);
        Task<UserDetail> GetUserByEmail(string email);
        Task<FullUserDetail> GetUserDetail(string id);
        Task<IEnumerable<UserDetail>> GetUsersByType(string userType);
        Task<IEnumerable<UserDetail>> FindUsers(string searchTerm);
    }
    public interface IAccountService
    {
        Task<Account> GetAccountById(string id);
        Task<Account> CheckAndGetAccount(string userEmail, string password);
    }
}
