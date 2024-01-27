namespace FoodWebApp.Backend.Requests;

public class UserPasswordUpdateRequest
{
    public string Username { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}