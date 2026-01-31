using API_Service.Models.DTO;

namespace API_Service.RepositoryLayer.Interface
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDetail>> GetAllUsersAsync();
        Task<UserDetail?> GetUserAsync(string id);
    }
}
