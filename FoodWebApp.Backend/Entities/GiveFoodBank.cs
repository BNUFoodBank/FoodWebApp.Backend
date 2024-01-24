namespace FoodWebApp.Backend.Entities;

public class GiveFoodBank : Record
{
    public string address { get; set; }
    public string alt_name { get; set; }
    public bool closed { get; set; }
    public string country { get; set; }
    public Charity charity { get; set; }
    public string created { get; set; }
    public string email { get; set; }
    public string lat_lng { get; set; }
    public string name { get; set; }
    public string network { get; set; }
    public string phone { get; set; }
    public Politics politics { get; set; }
    public string postcode { get; set; }
    public string secondary_phone { get; set; }
    public string slug { get; set; }
    public BankURLS urls { get; set; }
}

public class Charity
{
    public string registration_id { get; set; }
    public Uri register_url { get; set; }
}

public class Politics
{
    public string parliamentary_constituency { get; set; }
    public string mp { get; set; }
    public string mp_party { get; set; }
    public string ward { get; set; }
    public string district { get; set; }
    public URLs urls { get; set; }
}

public class URLs
{
    public Uri html { get; set; }
    public Uri self { get; set; }
}

public class BankURLS {
    public Uri html { get; set; }
    public Uri self { get; set; }
    public Uri homepage { get; set; }
    public Uri shopping_list { get; set; }
}