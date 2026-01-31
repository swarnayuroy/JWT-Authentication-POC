using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using web.Models;
using web.Models.ResponseModel;

namespace web.Repository
{
    public interface IWebRepository
    {
        Task<ResponseDetail> CheckCredential(Credential userCredential);
        Task<ResponseDetail> RegisterUser(Registration userRegistrationDetail);
    }
}
