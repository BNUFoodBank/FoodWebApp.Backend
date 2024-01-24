using FoodWebApp.Backend.Public;

namespace FoodWebApp.Backend.Util;

public class Filtering
{
   public static List<PublicFoodBank> FilterList(List<FoodBankEntity> banks, Dictionary<string, string> parameters)
   {
      List<PublicFoodBank> returning = [];
      foreach (var bank in banks)
      {
         var check = true;
         foreach (var (key, value) in parameters)
         {
            switch (key.ToLower())
            {
               case "name":
                  if (!bank.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase)) check = false;
                  break;
               case "address":
                  if (!bank.Address.Equals(value, StringComparison.CurrentCultureIgnoreCase)) check = false;
                  break;
               case "dietaryrestriction":
                  if (!bank.DietaryRestriction.Contains(value.ToLower())) check = false;
                  break;
               case "items":
                  if (!bank.Items.ContainsKey(value.ToLower())) check = false;
                  break;
            }
         }
         if (check)
         {
            returning.Add(bank.GetPublicFoodBank());
         }
      }

      return returning;
   }
}