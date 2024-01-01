namespace FoodWebApp.Backend.Endpoints;

public class FoodBankEndpoint : IEndPoint
{
    private SurrealDbClient _dbClient;

    public FoodBankEndpoint()
    {
        _dbClient = new SurrealDbClient("");
    }

    public HttpResponseMessage CreateFoodBank([FromHeader]ClaimsPrincipal claims,FoodBankCreationRequest request)
    {
        if (!claims.HasClaim("role", "admin") && !claims.HasClaim("role", "manager"))
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
        
        
        _dbClient.Connect().GetAwaiter().GetResult();
        var parameters = new Dictionary<string, object> { { "Name", request.Name }, { "Address", request.Address } };
        var check = _dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name OR Address EQUALS $Address", parameters).GetAwaiter().GetResult();

        if (!check.IsEmpty)
        {
            return new HttpResponseMessage(HttpStatusCode.Conflict);
        }

        _ = check;

       var fb = FoodBankEntity.Create(request.Name, request.Address, claims.Identity!.Name!);

       _dbClient.Create("FoodBank", fb).GetAwaiter().GetResult();

        return new HttpResponseMessage(HttpStatusCode.Created);
    }

    public HttpResponseMessage AddItems([FromHeader] ClaimsPrincipal claims, FoodBankUpdateItemsRequest request)
    {
        if (!claims.HasClaim("role", "admin") && !claims.HasClaim("role", "manager"))
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

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

    public HttpResponseMessage RemoveItems([FromHeader] ClaimsPrincipal claims, FoodBankUpdateItemsRequest request)
    {
        if (!claims.HasClaim("role", "admin") && !claims.HasClaim("role", "manager"))
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

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

    public HttpResponseMessage UpdatePage([FromHeader] ClaimsPrincipal claims,FoodBankPageUpdateRequest request)
    {
        var parameters = new Dictionary<string, object> {{"Name", request.Name}};
        var check = _dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $Name", parameters).GetAwaiter().GetResult();
        if (check.IsEmpty)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        var fb = check.GetValue<FoodBankEntity>(0)!;


        if (claims.Identity!.Name != fb.Manager && !claims.HasClaim("role", "admin"))
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

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
    
    protected override void AddEndpoints(WebApplication app)
    {
        app.MapPost("/foodbank/create", CreateFoodBank).RequireAuthorization();
        app.MapPost("/foodbank/items/add", AddItems).RequireAuthorization();
        app.MapPost("/foodbank/items/remove", RemoveItems).RequireAuthorization();
        app.MapPost("/foodbank/page", UpdatePage).RequireAuthorization();
        app.MapGet("/foodbank/page", GetPage).AllowAnonymous();
    }
}