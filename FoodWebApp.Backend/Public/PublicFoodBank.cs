namespace FoodWebApp.Backend.Public;

public class PublicFoodBank
{
    // Food Bank Data
    public string Name { get; set; }
    public string Address { get; set; }

    // Storage / Request Data
    public List<string> DietaryRestriction { get; set; } = new();
    public Dictionary<string, int> Items { get; set; } = new();

    public PublicFoodBank(string name, string address, List<string> dietaryRestriction, Dictionary<string, int> items)
    {
        Name = name;
        Address = address;
        DietaryRestriction = dietaryRestriction;
        Items = items;
    }
}