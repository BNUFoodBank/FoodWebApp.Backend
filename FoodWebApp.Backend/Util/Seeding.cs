using SurrealDb.Net.Models.Auth;

namespace FoodWebApp.Backend.Util;

public class Seeding
{
    public static async Task<bool> SeedDatabase()
    {
        var db = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await db.SignIn(new RootAuth() {Password = "root", Username = "root"});
        await db.Use("test", "test");
        
        await db.Delete("FoodBanks");

        var http = new HttpClient(new HttpClientHandler());

        var responseMessage = await http.GetAsync(@"https://www.givefood.org.uk/api/2/foodbanks/");

        var banks = await responseMessage.Content.ReadFromJsonAsync<List<GiveFoodBank>>();

        if (banks == null)
        {
            throw new NullReferenceException("No Banks Recieved");
        }

        foreach (var bank in banks.Select(giveFoodBank => new FoodBankEntity()
                 {
                     Name = giveFoodBank.name,
                     Address = giveFoodBank.address,
                     LatLng = giveFoodBank.lat_lng,
                     Description = string.Empty,
                     DietaryRestriction = new List<string>(),
                     Instructions = string.Empty,
                     Items = new Dictionary<string, int>(),
                     Manager = giveFoodBank.email,
                     RequestedItems = new Dictionary<string, int>(),
                     UserRequests = new Dictionary<string, List<string>>(),
                     ShoppingList = giveFoodBank.urls.shopping_list.ToString()
                 }))
        {
            await db.Create("FoodBanks", bank);
        }

        return true;
    }
}