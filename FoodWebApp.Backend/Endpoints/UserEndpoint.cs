﻿using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using FoodWebApp.Backend.Entities;
using FoodWebApp.Backend.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SurrealDb.Net;

namespace FoodWebApp.Backend.Endpoints;

public class UserEndpoint : IEndPoint
{

    private SurrealDbClient _dbClient;
    
    public UserEndpoint()
    {
        //_dbClient = new SurrealDbClient("");
        //_dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
    }
    
    public HttpResponseMessage Login(UserLoginRequest request)
    {
        _dbClient.Connect().GetAwaiter().GetResult();
        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var user = _dbClient.Query("SELECT * FROM type::table(\"Users\") WHERE Username EQUALS $user", parameters).GetAwaiter().GetResult().GetValue<UserEntity>(0);
        
        if (user == null) {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict);
            responseMessage.Content = new StringContent("Incorrect Username or Password.");
            return responseMessage;
        }

        if (!user.VerifyPassword(user.Salt, request.Password, user.Password)) {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict);
            responseMessage.Content = new StringContent("Incorrect Username or Password.");
            return responseMessage;
        }

        var token = GenerateJWTToken(request.Username, user.Id!.ToString(), TimeSpan.FromDays(30), ["user"]);
        
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent(token);
        return response;
    }

    public HttpResponseMessage Register(UserRegisterRequest request)
    {
        _dbClient.Connect().GetAwaiter().GetResult();
        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var userCheck = _dbClient.Query("SELECT * FROM type::table(\"Users\") WHERE Username EQUALS $user", parameters).GetAwaiter().GetResult().GetValue<UserEntity>(0);
        
        if (userCheck != null)
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict);
            responseMessage.Content = new StringContent("User Already Exists");
            return responseMessage;
        }

        var user = UserEntity.Create(request.Username, request.Password, request.ReferralCode);
        _dbClient.Create("Users", user).GetAwaiter().GetResult();
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Content = new StringContent("User Registered Successfully");
        return response;
    }
    
    public HttpResponseMessage Delete([FromHeader]ClaimsPrincipal userClaims, [FromBody]UserDeletionRequest request)
    {
        _dbClient.Connect().GetAwaiter().GetResult();
        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var userCheck = _dbClient.Query("SELECT * FROM type::table(\"Users\") WHERE Username EQUALS $user", parameters).GetAwaiter().GetResult().GetValue<UserEntity>(0);

        if (userCheck == null)
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            responseMessage.Content = new StringContent("User Not Found");
            return responseMessage;
        }
        
        var passwordVerified = userCheck.VerifyPassword(userCheck.Salt, request.Password, userCheck.Password);
        
        if (!passwordVerified) {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            responseMessage.Content = new StringContent("Invalid Password");
            return responseMessage;
        }
        
        _dbClient.Delete("Users", userCheck.Id!.ToString()).GetAwaiter().GetResult();
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent("User Deleted Successfully");
        return response;
    }

    protected override void AddEndpoints(WebApplication app)
    {
        app.MapPost("/user/login", Login);
        app.MapPost("/user/register", Register);
        app.MapPost("/user/delete", Delete).RequireAuthorization();
    }

    static string GenerateJWTToken(string username, string userId, TimeSpan expiration, string[] permissions)
    {
        
        var symmetricKey = new SymmetricSecurityKey("your_secret_key"u8.ToArray());
        
        var signingCredentials = new SigningCredentials(
            symmetricKey,

            SecurityAlgorithms.Sha512);

        var claims = new List<Claim>()
        {
            new Claim("sub", username),
            new Claim("name", userId),
            new Claim("aud", "Who cares?")
        };
    
        var roleClaims = permissions.Select(x => new Claim("role", x));
        claims.AddRange(roleClaims);

        var token = new JwtSecurityToken(
            issuer: "IDGAF",
            audience: "Who cares?",
            claims: claims,
            expires: DateTime.Now.Add(expiration),
            signingCredentials: signingCredentials);

        var rawToken = new JwtSecurityTokenHandler().WriteToken(token);
        return rawToken;
    }
}