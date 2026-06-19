using DataContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.DataService
{
    public interface IContextService
    {
        Task SaveUserAsync(User userDetail);
        Task SaveUserRoleAsync(UserRole userRole);
        Task SaveAccountAsync(Account accountDetail);
        Task UpdateUserAsync(User userDetail);
        Task UpdateUserRoleAsync(UserRole userRole);
        Task UpdateAccountAsync(Account accountDetail);
        Task DeleteUserAsync(User user);
    }
}
