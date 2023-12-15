using Microsoft.AspNetCore.Http.HttpResults;
using SurrealDb.Net.Models;

namespace FoodWebApp.Backend.Entities;

public class FoodBankEntity : Record
{
    public string Name { get; set; }
    public string Address { get; set; }
    public Dictionary<string, int> Items { get; set; }
    public Dictionary<string, int> Requests { get; set; }
    public string Manager { get; set; }

    public static FoodBankEntity Create(string name, string address, UserEntity manager) =>
        new()
        {
            Name = name,
            Address = address,
            Items = new Dictionary<string, int>(),
            Requests = new Dictionary<string, int>(),
            Manager = manager.Id!.ToString()
        };
}