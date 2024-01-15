using System.Text.Json;
using FactorySimulation.Interfaces;
public static class ResourceTransformerInfoExtensions
{
    public static string ToJson(this IResourceTransformerInfo t){
        var dictValues = new Dictionary<string,object>();

        dictValues["TransformationName"]=t.TransformationName;
        dictValues["InputResources"]=t.InputResources.Select(i=>new object[]{i.resourceName,i.amount});
        dictValues["OutputResources"]=t.OutputResources.Select(i=>new object[]{i.resourceName,i.amount});
        dictValues["Time"]=t.Time;
        dictValues["Price"]=t.Price;

        return JsonSerializer.Serialize(dictValues);
    }
    /// <summary>
    /// Converts many resources from single json string.<br/>
    /// </summary>
    /// <param name="json">Contains single array objects that can be converted by <see cref="ResourceTransformerInfo.FromJson(string)"/> </param>
    /// <returns></returns>
    public static string ManyToJson(this IResourceTransformerInfo[] t){
        return "["+string.Join(',',t.Select(i=>i.ToJson()))+"]";
    }
}
