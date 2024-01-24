namespace FoodWebApp.Backend.Util;

[JsonSerializable(typeof(HttpResponseMessage))]
[JsonSerializable(typeof(UserLoginRequest))]
[JsonSerializable(typeof(UserRegisterRequest))]
[JsonSerializable(typeof(UserDeletionRequest))]
[JsonSerializable(typeof(FoodBankCreationRequest))]
[JsonSerializable(typeof(FoodBankPageUpdateRequest))]
[JsonSerializable(typeof(FoodBankUpdateItemsRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}