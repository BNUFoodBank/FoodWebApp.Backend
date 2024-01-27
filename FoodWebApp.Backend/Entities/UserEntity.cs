namespace FoodWebApp.Backend.Entities;

public class UserEntity : Record
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];

    public string? ReferralCode { get; set; }

    public bool Banned { get; set; } = false;

    public string Role { get; set; } = "User";

    public static UserEntity Create(string username, string password, string? referralcode)
    {
        var (hashedPassword, salt) = HashPassword(password);
        return new UserEntity { Username= username, Password = hashedPassword, Salt = salt, ReferralCode = referralcode, Permissions =
            ["user"]
        };
    }
    
    public bool VerifyPassword(string salt, string password, string hashedPassword)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 2,
            MemoryCost = 65536,
            Lanes = 2,
            Password = passwordBytes,
            Salt = Convert.FromBase64String(salt),
            HashLength = 512,
            AssociatedData = "SomeAssociatedData"u8.ToArray()
        };

        using var argon2 = new Argon2(config);
        var argon = argon2.Hash().Buffer;
        return Convert.ToBase64String(argon) == hashedPassword;
    }
    
    public static Tuple<string, string> HashPassword(string password)
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
        return Tuple.Create(Convert.ToBase64String(hashResult.Buffer), Convert.ToBase64String(salt))!;
    }

    private static byte[] GenerateSalt()
    {
        var buffer = NewGuid();
        
        return buffer.ToByteArray();
    }
}