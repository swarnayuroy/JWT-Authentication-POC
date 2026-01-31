using API_Service.Models.DTO;
using API_Service.Models.ResponseModel;

namespace API_Service.RepositoryLayer.Interface
{
    public interface IAccountRepository
    {
        Task<ResponseDetail> CheckCredential(UserCredential userCredential);
        Task<ResponseDetail> RegisterUser(UserDetail userRegistrationDetail);
    }
}
