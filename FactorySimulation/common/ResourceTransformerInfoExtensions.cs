using FactorySimulation.Interfaces;
public static class ResourceTransformerInfoExtensions
{
    public static string ToJson(this IResourceTransformerInfo t)
        => Newtonsoft.Json.JsonConvert.SerializeObject(new{
            InputResources=t.InputResources.Select(i=>new{i.resourceName,i.amount}),
            OutputResources=t.OutputResources.Select(i=>new{i.resourceName,i.amount}),
            t.Time,
            t.Price
        });
    
}
