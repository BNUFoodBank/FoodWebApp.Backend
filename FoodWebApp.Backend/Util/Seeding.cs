using SurrealDb.Net.Models.Auth;

namespace FoodWebApp.Backend.Util;

public class Seeding
{
    public static bool SeedDatabase()
    {
        var db = new SurrealDbClient("127.0.0.1");
        db.SignIn(new RootAuth() {Password = "root", Username = "root"});
        
        db.Delete("FoodBank").GetAwaiter().GetResult();

        var http = new HttpClient(new HttpClientHandler());

        var responseMessage = http.GetAsync(@"https://www.givefood.org.uk/api/2/foodbanks/").GetAwaiter().GetResult();

        var banks = responseMessage.Content.ReadFromJsonAsync<List<GiveFoodBank>>().GetAwaiter().GetResult();

        if (banks == null)
        {
            throw new NullReferenceException("No Banks Recieved");
        }

        foreach (var bank in banks.Select(giveFoodBank => new FoodBankEntity()
                 {
                     Name = giveFoodBank.name,
                     Address = giveFoodBank.address,
                     Description = string.Empty,
                     DietaryRestriction = new List<string>(),
                     Instructions = string.Empty,
                     Items = new Dictionary<string, int>(),
                     Manager = giveFoodBank.email,
                     RequestedItems = new Dictionary<string, int>(),
                     UserRequests = new Dictionary<string, List<string>>(),
                 }))
        {
            db.Create(bank);
        }

        return true;
    }
}