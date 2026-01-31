using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace web.Models
{
    public class Credential
    {
        [Required(ErrorMessage = "Please enter your email")]
        [RegularExpression(@"^[a-z][\w.]+@[a-z]+\.[a-z]{3}$", ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        public string Password { get; set; }
    }
}