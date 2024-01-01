namespace FoodWebApp.Backend.Requests;

public class FoodBankPageUpdateRequest
{
   public string Name { get; set; }
   public string? Title { get; set; } 
   public string? Description { get; set; }
   public string? Instructions { get; set; }
}