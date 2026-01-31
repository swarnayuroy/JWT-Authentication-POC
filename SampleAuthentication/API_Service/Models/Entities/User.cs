using System.ComponentModel.DataAnnotations;

namespace API_Service.Models.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /* checks if the user have verified their respective account after registration */
        public bool? IsVerified { get; set; } = false;
    }
}
