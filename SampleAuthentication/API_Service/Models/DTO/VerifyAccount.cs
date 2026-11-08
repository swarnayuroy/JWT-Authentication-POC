namespace API_Service.Models.DTO
{
    public class LoginResult
    {
        public string Type { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Handoff { get; set; } = string.Empty;
    }
    public class VerifyAccount
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public bool IsLoggedIn { get; set; } = false;
    }
}
