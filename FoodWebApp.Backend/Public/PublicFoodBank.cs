namespace FoodWebApp.Backend.Public;

public class PublicFoodBank
{
    // Food Bank Data
    public string Name { get; set; }
    public string Address { get; set; }

    // Storage / Request Data
    public List<string> DietaryRestrictions { get; set; } = new();
    public Dictionary<string, int> Items { get; set; } = new();
    public string ShoppingList { get; set; } = string.Empty;
    public string LatLng { get; set; } = string.Empty;

    public PublicFoodBank(string name, string address, List<string> dietaryRestrictions, Dictionary<string, int> items, string latLng, string shoppingList)
    {
        Name = name;
        Address = address;
        DietaryRestrictions = dietaryRestrictions;
        Items = items;
        LatLng = latLng;
        ShoppingList = shoppingList;
    }
}