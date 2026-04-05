using System.Security.Cryptography;

namespace API_Service.Models.Entities
{
    public class UserOTP
    {
        private string _otp = string.Empty;
        private DateTime _otpGenerated;

        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string Otp 
        {
            get 
            {
                return this._otp;
            }
            set
            {
                value = Convert.ToString(RandomNumberGenerator.GetInt32(100000, 1000000));
                this._otp = value;
            }
        }
        public DateTime OtpGenerated 
        {
            get { return _otpGenerated; }
            set { value = DateTime.Now; _otpGenerated = value; }
        }

        public bool IsChecked { get; set; } = false;
        public bool IsSuccess { get; set; } = false;
    }
}