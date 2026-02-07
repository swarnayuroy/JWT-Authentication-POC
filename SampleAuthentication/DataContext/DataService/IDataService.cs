using DataContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.DataService
{
    public interface IDataService
    {
        Task SaveUserAsync(User userDetail);
        Task SaveAccountAsync(Account accountDetail);
        Task<bool> UpdateUserAsync(User userDetail);
        Task<bool> UpdateAccountAsync(Account accountDetail);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> DeleteAccountAsync(Guid accountId);
    }
}
