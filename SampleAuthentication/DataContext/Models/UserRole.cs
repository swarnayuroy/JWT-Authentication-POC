using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.Models
{
    public enum UserRoleType
    {
        User,
        Admin   
    }
    public class UserRole
    {
        public Guid UserId { get; set; }
        public UserRoleType Role { get; set; }
    }
}
