using DataContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.DataProvider
{
    public interface IDataProvider
    {
        IList<User> User { get; }
        IList<Account> Account { get; }
    }
}
