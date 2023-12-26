using FactorySimulation.Interfaces;
public static class ResourceTransformerInfoExtensions
{
    public static string ToJson(this IResourceTransformerInfo t)
        => Newtonsoft.Json.JsonConvert.SerializeObject(new{
            t.TransformationName,
            InputResources=t.InputResources.Select(i=>new{i.resourceName,i.amount}),
            OutputResources=t.OutputResources.Select(i=>new{i.resourceName,i.amount}),
            t.Time,
            t.Price
        });
    /// <summary>
    /// Converts many resources from single json string.<br/>
    /// </summary>
    /// <param name="json">Contains single array objects that can be converted by <see cref="ResourceTransformerInfo.FromJson(string)"/> </param>
    /// <returns></returns>
    public static string ManyToJson(this IResourceTransformerInfo[] t){
        return Newtonsoft.Json.JsonConvert.SerializeObject(t.Select(i=>i.ToJson()));
    }
}
