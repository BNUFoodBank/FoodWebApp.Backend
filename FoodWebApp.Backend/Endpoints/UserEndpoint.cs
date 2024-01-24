using SurrealDb.Net.Models.Auth;

namespace FoodWebApp.Backend.Endpoints;

public class UserEndpoint : IEndPoint
{

    private SurrealDbClient _dbClient;
    
    public UserEndpoint()
    {
        _dbClient = new SurrealDbClient("ws://127.0.0.1:4505");
        _dbClient.SignIn(new RootAuth {Username = "root", Password = "root"});
    }
    
    public string Login(UserLoginRequest request)
    {
        _dbClient.Connect().GetAwaiter().GetResult();
        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var user = _dbClient.Query("SELECT * FROM type::table(\"Users\") WHERE Username EQUALS $User", parameters).GetAwaiter().GetResult().GetValue<UserEntity>(0);
        
        if (user == null) {
            return "Incorrect Username or Password.";
        }
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

    public bool Register(UserRegisterRequest request)
    {
        _dbClient.Connect().GetAwaiter().GetResult();
        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var userCheck = _dbClient.Query("SELECT * FROM type::table(\"Users\") WHERE Username EQUALS $User", parameters).GetAwaiter().GetResult().GetValue<UserEntity>(0);
        
        if (userCheck != null)
        {
            return false;
        }

        var user = UserEntity.Create(request.Username, request.Password, request.ReferralCode);
        _dbClient.Create("Users", user).GetAwaiter().GetResult();
 
        return true;
    }
    
    public bool Delete([FromHeader]string userClaims, [FromBody]UserDeletionRequest request)
    {
        _dbClient.Connect().GetAwaiter().GetResult();
        var parameters = new Dictionary<string, object> { { "User", request.Username } };
        var userCheck = _dbClient.Query("SELECT * FROM type::table(\"Users\") WHERE Username EQUALS $User", parameters).GetAwaiter().GetResult().GetValue<UserEntity>(0);

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
            expires: Now.Add(expiration),
            signingCredentials: signingCredentials);

        var rawToken = new JwtSecurityTokenHandler().WriteToken(token);
        return rawToken;
    }
}