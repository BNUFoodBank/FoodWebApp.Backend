﻿namespace FoodWebApp.Backend.Util;

[JsonSerializable(typeof(HttpResponseMessage))]
[JsonSerializable(typeof(UserLoginRequest))]
[JsonSerializable(typeof(UserRegisterRequest))]
[JsonSerializable(typeof(UserDeletionRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}