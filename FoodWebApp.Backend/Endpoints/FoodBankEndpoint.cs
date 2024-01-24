using SurrealDb.Net.Models.Auth;

namespace FoodWebApp.Backend.Endpoints;

public class FoodBankEndpoint : IEndPoint
{
    private SurrealDbClient _dbClient;

    public FoodBankEndpoint()
    {
        _dbClient = new SurrealDbClient("ws://127.0.0.1:4505");
        _dbClient.SignIn(new RootAuth() {Password = "root", Username = "root"});
    }

    public HttpResponseMessage CreateFoodBank([FromHeader]string claims,FoodBankCreationRequest request)
    {
        
        _dbClient.Connect().GetAwaiter().GetResult();
        var parameters = new Dictionary<string, object> { { "Name", request.Name }, { "Address", request.Address } };
        var check = _dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name OR Address EQUALS $Address", parameters).GetAwaiter().GetResult();

        if (!check.IsEmpty)
        {
            return new HttpResponseMessage(HttpStatusCode.Conflict);
        }

        _ = check;

       var fb = FoodBankEntity.Create(request.Name, request.Address, request.Name);

       _dbClient.Create("FoodBank", fb).GetAwaiter().GetResult();

        return new HttpResponseMessage(HttpStatusCode.Created);
    }

    public HttpResponseMessage AddItems([FromHeader]string claims, FoodBankUpdateItemsRequest request)
    {

        var parameters = new Dictionary<string, object> {{"Name", request.FoodBankName}};
        var check = _dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();

        if (!check.IsEmpty)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        var fb = check.GetValue<FoodBankEntity>(0)!;
        
        foreach (var (item, quantity) in request.Items)
        {
            fb.Items.Add(item, quantity);
        }

        _dbClient.Upsert(fb).GetAwaiter().GetResult();

        return new HttpResponseMessage(HttpStatusCode.Accepted);
    }

    public HttpResponseMessage RemoveItems([FromHeader] string claims, FoodBankUpdateItemsRequest request)
    {

        var parameters = new Dictionary<string, object> {{"Name", request.FoodBankName}};
        var check = _dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();

        if (!check.IsEmpty)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
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

        _dbClient.Upsert(fb).GetAwaiter().GetResult();

        return new HttpResponseMessage(HttpStatusCode.Accepted);
    }


    public HttpResponseMessage GetPage(string name)
    {
        var parameters = new Dictionary<string, object> {{"Name", name}};
        var check = _dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();

        return check.IsEmpty ? new HttpResponseMessage(HttpStatusCode.NotFound) : check.GetValue<FoodBankEntity>(0)!.GetPage();
    }

    public HttpResponseMessage UpdatePage([FromHeader] string claims,FoodBankPageUpdateRequest request)
    {
        var parameters = new Dictionary<string, object> {{"Name", request.Name}};
        var check = _dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();
        if (check.IsEmpty)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
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

        _ = _dbClient.Upsert(fb).GetAwaiter().GetResult();

        return new HttpResponseMessage(HttpStatusCode.Accepted);
    }

    public HttpResponseMessage FilterBanks(Dictionary<string, string> filter)
    {
        _dbClient.Connect().GetAwaiter().GetResult();

        var foodbanks = _dbClient.Select<FoodBankEntity>("FoodBank").GetAwaiter().GetResult();

        var filteredList = Filtering.FilterList(foodbanks.ToList(), filter);

        var message = new HttpResponseMessage(HttpStatusCode.Found);
        message.Content = new StringContent(filteredList.ToString() ?? string.Empty);
        return message;
    }
    
    protected override void AddEndpoints(WebApplication app)
    {
        app.MapPost("/foodbank/create", CreateFoodBank).RequireAuthorization();
        app.MapPost("/foodbank/items/add", AddItems).RequireAuthorization();
        app.MapPost("/foodbank/items/remove", RemoveItems).RequireAuthorization();
        app.MapPost("/foodbank/page", UpdatePage).RequireAuthorization();
        app.MapGet("/foodbank/page", GetPage).AllowAnonymous();
    }
}