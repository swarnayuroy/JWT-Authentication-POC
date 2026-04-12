using API_Service.Models.Entities;
using System.Security.Cryptography;

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
            var newOTPrecord = new UserOTP
            {
                Otp = Convert.ToString(RandomNumberGenerator.GetInt32(100000, 1000000)),
                OtpGenerated = DateTime.Now,
                IsChecked = true
            };
            var record = _userOTPs.FirstOrDefault(u => u.UserId == userId);

            // if record exists, update the OTP and OTP generated time
            if (record != null)
            {
                record.Otp = newOTPrecord.Otp;
                record.OtpGenerated = newOTPrecord.OtpGenerated;
                _userOTPs[_userOTPs.IndexOf(_userOTPs.First(otp => otp.UserId == userId))] = record;

                return record.Otp;
            }

            // if record does not exist, create new otp record then add to the list
            newOTPrecord.UserId = userId;
            newOTPrecord.UserEmail = userEmail;

            _userOTPs.Add(newOTPrecord);

            return newOTPrecord.Otp;
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
