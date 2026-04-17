using API_Service.Models.Entities;
using DataContext.Models;
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
            var new_otp_record = new UserOTP(userId, userEmail);
            var record = _userOTPs.FirstOrDefault(u => u.UserId == userId);

            // if record exists, update the OTP record with new OTP
            if (record != null)
            {
                record = new_otp_record;
                record.IsChecked = true;
                return UpdateOtpRecord(record) ? record.Otp : string.Empty;
            }

            // if record does not exist just add new otp record to the list
            new_otp_record.IsChecked = true;
            AddOtpRecord(new_otp_record);

            return new_otp_record.Otp;
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
                return UpdateOtpRecord(record);
            }
            return false;
        }

        // below method will clear the OTP record for the user after successful password reset
        public static void ClearOtp(string email)
        {
            var record = _userOTPs.FirstOrDefault(u => u.UserEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (record != null)
            {
                RemoveOtpRecord(record);
            }
        }

        #region OTP record service
        public static void AddOtpRecord(UserOTP otpRecord)
        {
            _userOTPs.Add(otpRecord);
        }

        public static bool UpdateOtpRecord(UserOTP otpRecord)
        {
            _userOTPs[_userOTPs.IndexOf(_userOTPs.First(otp => otp.UserId == otpRecord.UserId))] = otpRecord;
            return true;
        }

        public static void RemoveOtpRecord(UserOTP otpRecord)
        {
            _userOTPs.Remove(otpRecord);
        }
        #endregion
    }
}
