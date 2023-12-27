using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using FactorySimulation.Interfaces;

namespace FactorySimulation;

///<inheritdoc/>
public record ResourceTransformerInfo(string TransformationName,(string resourceName, long amount)[] InputResources, (string resourceName, long amount)[] OutputResources, long Time = 1, long Price = 0) : IResourceTransformerInfo{
    string serialize((string resourceName, long amount)[] data){
        return JsonSerializer.Serialize(data.Select(x=>new object[]{x.resourceName,x.amount}))[1..^1];
    }
    ///<inheritdoc/>
    public override string ToString()
    {
        if(InputResources.Zip(OutputResources).All(x=>x.First.resourceName==x.Second.resourceName && x.First.amount==x.Second.amount))
            return $"{TransformationName}\n{serialize(InputResources)}";
        return $"{TransformationName}\n{serialize(InputResources)}\n{serialize(OutputResources)}\nTime: {Time}";
    }
    /// <summary>
    /// Converts this object to json
    /// </summary>
    public string ToJson() => ResourceTransformerInfoExtensions.ToJson(this);
    /// <summary>
    /// Creates resource transformer from json
    /// </summary>
    public static IResourceTransformerInfo FromJson(string json){
        dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json) ?? throw new ArgumentException("Possible empty json string");
        var InputResources =  (obj["InputResources"] as IEnumerable<dynamic>  ?? throw new ArgumentException("Missing array InputResources")).Select(i=>((string)i[0],(long)i[1]));
        var OutputResources = (obj["OutputResources"] as IEnumerable<dynamic> ?? throw new ArgumentException("Missing array OutputResources")).Select(i=>((string)i[0],(long)i[1]));
        var Time = (long)obj["Time"];
        var Price = (long)obj["Price"];
        var TransformationName = (string)obj["TransformationName"];
        return new ResourceTransformerInfo(TransformationName,InputResources.ToArray(),OutputResources.ToArray(),Time,Price);
    }
    /// <summary>
    /// Converts many resources from single json string.<br/>
    /// </summary>
    /// <param name="json">Contains single array objects that can be converted by <see cref="ResourceTransformerInfo.FromJson(string)"/> </param>
    /// <returns></returns>
    public static IResourceTransformerInfo[] ManyFromJson(string json){
        dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json) ?? throw new ArgumentException("Possible empty json string");
        var many = obj as IEnumerable<dynamic> ?? throw new ArgumentException("Json does not contains top level array");
        return many.Select(str=>FromJson((string)str.ToString())).ToArray();
    }
};


