using API_Service.Models.ResponseModel;

namespace API_Service.RepositoryLayer.Interface
{
    public interface IUserRepository
    {
        Task<ResponseDetail> GetAllUsersAsync(string userId, int page, int pageSize);
        Task<ResponseDetail> GetUserBySearch(string userId, int page, int pageSize, string searchText);
        Task<ResponseDetail> GetUserAsync(string id);
        Task<ResponseDetail> GetUserDetailAsync(string id);
    }
}
