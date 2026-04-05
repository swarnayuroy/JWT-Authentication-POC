namespace API_Service.Models.DTO
{
    public class VerifyAccount
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
