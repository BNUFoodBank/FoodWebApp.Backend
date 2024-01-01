namespace FoodWebApp.Backend.Requests;

public class FoodBankUpdateItemsRequest
{
    public string FoodBankName { get; set; }
    public Dictionary<string, int> Items { get; set; }
}