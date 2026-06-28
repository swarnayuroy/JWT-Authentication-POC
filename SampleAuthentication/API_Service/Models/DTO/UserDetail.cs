using System.Security.Cryptography.X509Certificates;

namespace API_Service.Models.DTO
{
    public class UserCredential
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class UserDetail
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool? IsVerified { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class FullUserDetail : UserDetail
    {
        private string _accountOld = string.Empty;
        public DateTime? LoggedInAt { get; set; }
        public string AccountOld {            
            get
            {
                return _accountOld;
            }
            set
            {
                if (DateTime.Parse(value) > DateTime.MinValue)
                {
                    var accountAge = DateTime.UtcNow - DateTime.Parse(value);
                    // Less than 24 hours - return in hours
                    if (accountAge.TotalHours < 24)
                    {
                        if (accountAge.TotalHours < 1)
                        {
                            value = "few minutes ago";
                        }
                        else
                        {
                            value = $"{(int)accountAge.TotalHours} hour{((int)accountAge.TotalHours != 1 ? "s" : "")}";
                        }
                        
                    }
                    // Less than 30 days - return in days
                    else if (accountAge.TotalDays < 30)
                    {
                        value = $"{accountAge.Days} day{(accountAge.Days != 1 ? "s" : "")}";
                    }
                    // Less than 12 months - return in months
                    else if (accountAge.TotalDays < 365)
                    {
                        var months = (int)(accountAge.TotalDays / 30.44); // Average days per month
                        value = $"{months} month{(months != 1 ? "s" : "")}";
                    }
                    // 12 months or more - return in years
                    else
                    {
                        var years = (int)(accountAge.TotalDays / 365.25); // Average days per year
                        value = $"{years} year{(years != 1 ? "s" : "")}";
                    }                    
                }
                else
                {
                    value = "NA";
                }
                _accountOld = value;
            }
        }
    }
}
