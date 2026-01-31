using System.ComponentModel.DataAnnotations;

namespace API_Service.Models.Entities
{
    public class Account
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; } = string.Empty;

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
