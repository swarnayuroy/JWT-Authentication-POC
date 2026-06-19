using DataContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.DataProvider
{
    public interface IContextProvider
    {
        IQueryable<User> User { get; }
        IQueryable<UserRole> UserRole { get; }
        IQueryable<Account> Account { get; }
    }
}
