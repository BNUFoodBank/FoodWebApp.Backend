using FoodWebApp.Backend.Public;

namespace FoodWebApp.Backend.Entities;

public class FoodBankEntity : Record
{
    // Food Bank Data
    public string Name { get; set; }
    public string Address { get; set; }
    
    // Storage / Request Data
    public List<string> DietaryRestriction { get; set; } = new();
    public Dictionary<string, int> Items { get; set; } = new();
    public Dictionary<string, int> RequestedItems { get; set; }
    
    public Dictionary<string, List<string>> UserRequests { get; set; }
    
    // Manage of the food bank
    public string Manager { get; set; }


    // Page Properties
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    
   // Functions 

    public static FoodBankEntity Create(string name, string address, string manager) =>
        new()
        {
            Name = name,
            Address = address,
            Items = new Dictionary<string, int>(),
            RequestedItems = new Dictionary<string, int>(),
            Manager = manager,
            UserRequests = new Dictionary<string, List<string>>()
        };


    public string GetPage()
    {
        return $"Title:{Title}$32Description:{Description}$32Instructions:{Instructions}";
    }
    
    
    // Convert FoodBank to Public
    public PublicFoodBank GetPublicFoodBank()
    {
        var pfb = new PublicFoodBank(Name, Address, DietaryRestriction, Items);
        return pfb;
    }

    
}