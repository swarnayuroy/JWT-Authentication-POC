using API_Service.Models.DTO;
using API_Service.Models.ResponseModel;

namespace API_Service.RepositoryLayer.Interface
{
    public interface IAccountRepository
    {
        Task<ResponseDetail> CheckCredential(UserCredential userCredential);
        Task<ResponseDetail> RegisterUser(UserDetail userRegistrationDetail);
        Task<ResponseDetail> DeleteAccount(string userId);
        Task<ResponseDetail> EmailExists(string email);
        Task<ResponseDetail> Verify(VerifyAccount detail);
        Task<ResponseDetail> SetPassword(UserCredential userCredential);
    }
}
