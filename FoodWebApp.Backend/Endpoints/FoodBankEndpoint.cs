using FoodWebApp.Backend.Public;
using SurrealDb.Net.Models.Auth;

namespace FoodWebApp.Backend.Endpoints;

public class FoodBankEndpoint : IEndPoint
{
    

    public FoodBankEndpoint()
    {
    }

    public async  Task<List<PublicFoodBank>> GetFoodBanks()
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");
        var foodBankEntities = await dbClient.Select<FoodBankEntity>("FoodBanks");

        return foodBankEntities.Select(foodBankEntity => foodBankEntity.GetPublicFoodBank()).ToList();
    }

    public async Task<bool> CreateFoodBank([FromHeader]string claims,FoodBankCreationRequest request)
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");
        var parameters = new Dictionary<string, object> { { "Name", request.Name }, { "Address", request.Address } };
        var check = dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name OR Address EQUALS $Address", parameters).GetAwaiter().GetResult();

        if (!check.IsEmpty)
        {
            return false;
        }

        _ = check;

       var fb = FoodBankEntity.Create(request.Name, request.Address, request.Name);

       dbClient.Create("FoodBank", fb).GetAwaiter().GetResult();

       return true;
    }

    public async Task<bool> AddItems([FromHeader]string claims, FoodBankUpdateItemsRequest request)
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");
        var parameters = new Dictionary<string, object> {{"Name", request.FoodBankName}};
        var check = dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();

        if (!check.IsEmpty)
        {
            return false;
        }

        var fb = check.GetValue<FoodBankEntity>(0)!;
        
        foreach (var (item, quantity) in request.Items)
        {
            fb.Items.Add(item, quantity);
        }

        dbClient.Upsert(fb).GetAwaiter().GetResult();

        return true;
    }

    public async Task<bool> RemoveItems([FromHeader] string claims, FoodBankUpdateItemsRequest request)
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");
        

        var parameters = new Dictionary<string, object> {{"Name", request.FoodBankName}};
        var check = dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();

        if (!check.IsEmpty)
        {
            return false;
        }

        var fb = check.GetValue<FoodBankEntity>(0)!;
        
        foreach (var (item, quantity) in request.Items)
        {
            if (!fb.Items.ContainsKey(item))
            {
                continue;
            }
            
            if (quantity == 0)
            {
                fb.Items.Remove(item);
                continue;
            }

            fb.Items[item] -= quantity;
        }

        dbClient.Upsert(fb).GetAwaiter().GetResult();

        return true;
    }


    public async Task<string> GetPage(string name)
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");
        
        var parameters = new Dictionary<string, object> {{"Name", name}};
        var check = dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();

        return check.GetValue<FoodBankEntity>(0)!.GetPage();
    }

    public async Task<bool> UpdatePage([FromHeader] string claims,FoodBankPageUpdateRequest request)
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");
        
        var parameters = new Dictionary<string, object> {{"Name", request.Name}};
        var check = dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();
        if (check.IsEmpty)
        {
            return false;
        }

        var fb = check.GetValue<FoodBankEntity>(0)!;

        if (!request.Description.IsNullOrEmpty())
        {
            fb.Description = request.Description!;
        }

        if (!request.Instructions.IsNullOrEmpty())
        {
            fb.Instructions = request.Instructions!;
        }

        if (!request.Title.IsNullOrEmpty())
        {
            fb.Instructions = request.Title!;
        }

        _ = dbClient.Upsert(fb).GetAwaiter().GetResult();

        return false;
    }

    public async Task<List<PublicFoodBank>> FilterBanks(Dictionary<string, string> filter)
    {
        var dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");

        var foodbanks = dbClient.Select<FoodBankEntity>("FoodBank").GetAwaiter().GetResult();

        var filteredList = Filtering.FilterList(foodbanks.ToList(), filter);
        
        return filteredList;
    }
    
    protected override void AddEndpoints(WebApplication app)
    {
        app.MapPost("/foodbank/create", CreateFoodBank).RequireAuthorization();
        app.MapPost("/foodbank/items/add", AddItems).RequireAuthorization();
        app.MapPost("/foodbank/items/remove", RemoveItems).RequireAuthorization();
        app.MapPost("/foodbank/page", UpdatePage).RequireAuthorization();
        app.MapGet("/foodbank/page", GetPage).AllowAnonymous();
        app.MapPost("/foodbank/filter", FilterBanks).AllowAnonymous();
        app.MapGet("/foodbanks", GetFoodBanks).AllowAnonymous();
    }
}