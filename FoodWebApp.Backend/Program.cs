using System.Security.Cryptography;
using FoodWebApp.Backend.Endpoints;
using FoodWebApp.Backend.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
        IssuerSigningKey = new SymmetricSecurityKey("your_secret_key"u8.ToArray()),
        ValidAlgorithms = new []{"SHA512"}
    };
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

IEndPoint.AddAllEndpoints(app);

app.Run();