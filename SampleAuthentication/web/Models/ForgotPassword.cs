using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace web.Models
{
    public class CheckEmail
    {
        [Required(ErrorMessage = "Please enter your email")]
        [RegularExpression(@"^[a-z][\w.]+@[a-z]+\.[a-z]{3}$", ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }
    }

    public class VerifyOTP
    {
        [Required(ErrorMessage = "Please enter the OTP")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Please enter a valid 6-digit OTP")]
        [Display(Name = "Enter OTP")]
        public string OTP { get; set; }
    }

    public class SetNewPassword
    {
        [Required(ErrorMessage = "Please enter a new password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your new password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPassword
    {
        public bool showEmail_Field { get; set; }
        public bool showOTP_Field { get; set; }
        public bool showSetPassword_Field { get; set; }
        public CheckEmail Email_Field { get; set; }
        public VerifyOTP OTP_Field { get; set; }
        public SetNewPassword SetPassword_Field { get; set; }
    }
}