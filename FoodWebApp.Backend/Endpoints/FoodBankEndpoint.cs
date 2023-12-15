using System.Net;
using System.Security.Claims;
using FoodWebApp.Backend.Entities;
using FoodWebApp.Backend.Requests;
using Microsoft.AspNetCore.Mvc;
using SurrealDb.Net;

namespace FoodWebApp.Backend.Endpoints;

public class FoodBankEndpoint : IEndPoint
{
    private SurrealDbClient _dbClient;
    public HttpResponseMessage CreateFoodBank([FromHeader]ClaimsPrincipal claims,FoodBankCreationRequest request)
    {
        if (!claims.HasClaim("role", "admin") && !claims.HasClaim("role", "manager"))
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
        
        
        _dbClient.Connect().GetAwaiter().GetResult();

        var check = _dbClient.Query("SELECT * FROM type::table(\"FoodBank\") WHERE Name EQUALS $name OR Address EQUALS $address").GetAwaiter().GetResult();

        if (!check.IsEmpty)
        {
            return new HttpResponseMessage(HttpStatusCode.Conflict);
        }

        _ = check;

        var fb = new FoodBankEntity
        {
            Name = request.Name,
            Address = request.Address,
            Items = new(),
            Requests = new(),
            Manager = claims.Identity!.Name!
        };

        var creation = _dbClient.Create("FoodBank", fb).GetAwaiter().GetResult();

        return new HttpResponseMessage(HttpStatusCode.Created);
    }
    
    protected override void AddEndpoints(WebApplication app)
    {
        app.MapPost("/foodbank/create", CreateFoodBank).RequireAuthorization();
    }
}