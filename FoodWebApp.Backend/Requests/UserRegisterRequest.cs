namespace FoodWebApp.Backend.Requests;

public class UserRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ReferralCode { get; set; } = string.Empty;
}
