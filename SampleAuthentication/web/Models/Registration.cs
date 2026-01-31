using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace web.Models
{
    public class Registration
    {
        [Required(ErrorMessage = "Please enter your name")]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "Name should be of length 30")]
        [RegularExpression(@"^[A-Z]{1}[a-z]+((\s[A-Z]{1}[a-z]*)*)?$", ErrorMessage = "Please enter a correct name.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [RegularExpression(@"^[a-z][\w.]+@[a-z]+\.[a-z]{3}$", ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a password")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "Password should be of length 8-15 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).*$", ErrorMessage = "Please enter a valid password.")]
        public string Password { get; set; }
    }
}