using System.Security.Cryptography;

namespace API_Service.Models.Entities
{
    public class UserOTP
    {
        private Guid _userId;
        private string _userEmail = string.Empty;
        private string _otp = string.Empty;
        private DateTime _otpGenerated;
        public UserOTP(Guid id, string email)
        {
            this._userId = id;
            this._userEmail = email;
            this._otp = Convert.ToString(RandomNumberGenerator.GetInt32(100000, 1000000));
            this._otpGenerated = DateTime.Now;
        }
        

        public Guid UserId { get {return this._userId; } }
        public string UserEmail { get {return this._userEmail; } }
        public string Otp 
        {
            get 
            {
                return this._otp;
            }
        }
        public DateTime OtpGenerated 
        {
            get { return _otpGenerated; }
        }

        public bool IsChecked { get; set; } = false;
        public bool IsSuccess { get; set; } = false;
    }
}