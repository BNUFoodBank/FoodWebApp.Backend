﻿namespace FoodWebApp.Backend.Requests;

public class UserLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}