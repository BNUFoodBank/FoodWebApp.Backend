using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using SurrealDb.Net.Models.Auth;

namespace FoodWebApp.Backend.Endpoints;


public class UserEndpoint : IEndPoint
{
    
    public UserEndpoint()
    {
       
    }
    
    static string ToJsonString(object? o)
    {
        return JsonSerializer.Serialize(o, new JsonSerializerOptions { WriteIndented = true, });
    }
    
    public async Task<string> Login(UserLoginRequest request)
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");
        var userCheck = await dbClient.Select<UserEntity>("Users");

        var userEntities = userCheck.ToList();
        if (userEntities.Count(x => x.Username == request.Username) == 0)
        {
            return "Incorrect Username or Password.";
        }

        var user = userEntities.First(x => x.Username == request.Username);
        if (user.Banned)
        {
            return "Incorrect Username or Password.";
        }
        
        if (!user.VerifyPassword(user.Salt, request.Password, user.Password)) {
            return "Incorrect Username or Password.";
        }

        var rawToken = GenerateJWTToken(request.Username, user.Id!.ToString(), FromDays(30), user.Permissions.ToArray());

        return rawToken+"#"+ user.Role;
    }

    public async Task<string> Register(UserRegisterRequest request)
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");

        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var userCheck = await dbClient.Select<UserEntity>("Users");
        
        if (userCheck.Count(x => x.Username == request.Username) == 1)
        {
            return "Failed To Register";
        }

        var user = UserEntity.Create(request.Username, request.Password, request.ReferralCode);
        await dbClient.Create("Users", user);
        return "Successfully Registered";
    }
    
    public bool Delete([FromHeader]string userClaims, [FromBody]UserDeletionRequest request)
    {
        var  _dbClient = new SurrealDbClient("ws://127.0.0.1:8000");
        _dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        _dbClient.Use("test", "test");
        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var userCheck = _dbClient.Query("SELECT * FROM type::table(\"Users\") WHERE Username = $User", parameters).GetAwaiter().GetResult().GetValue<UserEntity>(0);

        if (userCheck == null)
        {
            return false;
        }
        
        var passwordVerified = userCheck.VerifyPassword(userCheck.Salt, request.Password, userCheck.Password);
        
        if (!passwordVerified) {
            return false;
        }
        
        _dbClient.Delete("Users", userCheck.Id!.ToString()).GetAwaiter().GetResult();

        return true;
    }
    
    public async Task<string> UpdatePassword([FromBody] UserPasswordUpdateRequest request)
    {
        var  dbClient = new SurrealDbClient("ws://127.0.0.1:8000/rpc");
        await dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
        await dbClient.Use("test", "test");

        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var userCheck = await dbClient.Select<UserEntity>("Users");
        
        var user = userCheck.First(x => x.Username == request.Username);
        
        if (!user.VerifyPassword(user.Salt, request.OldPassword, user.Password))
        {
            return "Invalid Password.";
        }

        var (password, salt)= UserEntity.HashPassword(request.NewPassword);

        user.Salt = salt;
        user.Password = password;

        await dbClient.Upsert(user);
        
        
        return "Password Update";
    }

    protected override void AddEndpoints(WebApplication app)
    {
        app.MapPost("/user/login", Login);
        app.MapPost("/user/register", Register);
        app.MapPost("/user/delete", Delete).RequireAuthorization();
        app.MapPost("/user/updatepassword", UpdatePassword);
    }

    static string GenerateJWTToken(string username, string userId, TimeSpan expiration, string[] permissions)
    {
        
        var symmetricKey = new SymmetricSecurityKey("your_secret_key_is_very_safe_this_was_so_much_fun_to_write_please_keep_this_going_I_should_have_just_auto_generated_this_key_cos_its_now_very_Long"u8.ToArray());

        var signingCredentials = new SigningCredentials(
            symmetricKey,

            SecurityAlgorithms.HmacSha512);

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
            expires: Now.Add(expiration),
            signingCredentials: signingCredentials);

        var rawToken = new JwtSecurityTokenHandler().WriteToken(token);
        return rawToken;
    }
}