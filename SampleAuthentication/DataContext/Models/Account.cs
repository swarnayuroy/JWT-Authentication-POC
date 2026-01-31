using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; }

        /*
         sets date and time of new registration
        */
        public DateTime CreatedAt { get; set; }

        /*
         sets date an time of an user sign-in
        */
        public DateTime LoggedInAt { get; set; }
    }
}
