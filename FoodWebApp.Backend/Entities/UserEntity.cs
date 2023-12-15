using System;
using System.Text;
using Isopoh.Cryptography.Argon2;
using SurrealDb.Net.Models;
using static System.Guid;

namespace FoodWebApp.Backend.Entities;

public class UserEntity : Record
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    public string? ReferralCode { get; set; }

    public static UserEntity Create(string username, string password, string? referralcode)
    {
        var (hashedPassword, salt) = HashPassword(password);
        return new UserEntity { Username= username, Password = hashedPassword, Salt = salt, ReferralCode = referralcode};
    }
    
    public bool VerifyPassword(string salt, string password, string hashedPassword)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 2,
            MemoryCost = 65536,
            Lanes = 2,
            Password = passwordBytes,
            Salt = Encoding.UTF8.GetBytes(salt),
            HashLength = 512,
            AssociatedData = "SomeAssociatedData"u8.ToArray()
        };

        using var argon2 = new Argon2(config);
        return argon2.Hash().Buffer == hashedPasswordBytes;
    }
    
    private static Tuple<string, string> HashPassword(string password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var salt = GenerateSalt();
        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 2,
            MemoryCost = 65536,
            Lanes = 2,
            Password = passwordBytes,
            Salt = salt,
            HashLength = 512,
            AssociatedData = "SomeAssociatedData"u8.ToArray()
        };

        using var argon2 = new Argon2(config);
        var hashResult = argon2.Hash();
        return Tuple.Create(Convert.ToBase64String(hashResult.Buffer), salt.ToString())!;
    }

    private static byte[] GenerateSalt()
    {
        var buffer = NewGuid();
        
        return buffer.ToByteArray();
    }
}