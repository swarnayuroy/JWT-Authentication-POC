using API_Service.Models.ResponseModel;

namespace API_Service.RepositoryLayer.Interface
{
    public interface IUserRepository
    {
        Task<ResponseDetail> GetAllUsersAsync(string userId, int page, int pageSize);
        Task<ResponseDetail> GetUserAsync(string id);
    }
}
