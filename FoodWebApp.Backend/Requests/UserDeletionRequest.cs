namespace FoodWebApp.Backend.Requests;

public class UserDeletionRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}