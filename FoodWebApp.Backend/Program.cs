using FoodWebApp.Backend.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FoodWebApp.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "your_issuer",
                ValidAudience = "your_audience",
                IssuerSigningKey = new SymmetricSecurityKey("your_secret_key_is_very_safe_this_was_so_much_fun_to_write_please_keep_this_going_I_should_have_just_auto_generated_this_key_cos_its_now_very_Long"u8.ToArray()),
                ValidAlgorithms = new []{"HmacSha512"}
            };
        });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        IEndPoint.AddAllEndpoints(app);
        app.Run();
    }
}