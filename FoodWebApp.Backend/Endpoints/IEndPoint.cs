namespace FoodWebApp.Backend.Endpoints;

public abstract class IEndPoint : IComparable<IEndPoint>
{
    protected virtual void AddEndpoints(WebApplication app)
    {
        throw new NotImplementedException();
    }

    public static void AddAllEndpoints(WebApplication app)
    {
        var endpoints = ReflectiveEnumerator.GetEnumerableOfType<IEndPoint>();

        var endPoints = endpoints.ToList();
        if (endPoints.Count == 0)
        {
            throw new Exception("No Endpoints Found");
        }
        
        foreach (var endPoint in endPoints)
        {
            endPoint.AddEndpoints(app);
            Console.WriteLine("Added: " + endPoint.GetType().Name);
        }
    }

    public int CompareTo(IEndPoint? other)
    {
        return 0;
    }
}