using API_Service.Models.Entities;

namespace API_Service.Utils
{
    public class ProcessOtpService
    {
        private static List<UserOTP> _userOTPs = new List<UserOTP>();

        // below method will ensure to go ahead with setting new password for valid success status of OTP validation
        public static bool GetSuccessStatus(Guid userId, string email)
        {
            var record = _userOTPs.FirstOrDefault(u => u.UserId == userId && u.UserEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
            return record != null ? record.IsSuccess : false;
        }

        // below method will generate new OTP for the user or update the existing record by generating new OTP
        public static string GenerateOtp(Guid userId, string userEmail)
        {
            var record = _userOTPs.FirstOrDefault(u => u.UserId == userId);
            if (record != null)
            {
                var userOtp = new UserOTP();
                record.Otp = userOtp.Otp;
                record.OtpGenerated = userOtp.OtpGenerated;
                _userOTPs[_userOTPs.IndexOf(_userOTPs.First(otp => otp.UserId == userId))] = record;

                return record.Otp;
            }

            var newRecord = new UserOTP
            {
                UserId = userId,
                UserEmail = userEmail,
                IsChecked = true
            };
            _userOTPs.Add(newRecord);

            return newRecord.Otp;
        }

        // below method will validate the OTP for the user and update the success status if the OTP is valid
        public static bool ValidateOtp(string email, string otp)
        {
            var record = _userOTPs.FirstOrDefault(
                                u => u.UserEmail.Equals(email, StringComparison.OrdinalIgnoreCase)
                                && u.Otp.Equals(otp, StringComparison.OrdinalIgnoreCase)
                            );
            if (record != null)
            {
                record.IsSuccess = true;
                _userOTPs[_userOTPs.IndexOf(_userOTPs.First(otp => otp.UserId == record.UserId))] = record;
                return true;
            }
            return false;
        }

        // below method will clear the OTP record for the user after successful password reset
        public static void ClearOtp(string email)
        {
            var record = _userOTPs.FirstOrDefault(u => u.UserEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (record != null)
            {
                _userOTPs.Remove(record);
            }
        }
    }
}
