using API_Service.Models.ResponseModel;

namespace API_Service.RepositoryLayer.Interface
{
    public interface IUserRepository
    {
        Task<ResponseDetail> GetAllUsersAsync();
        Task<ResponseDetail> GetUserAsync(string id);
    }
}
