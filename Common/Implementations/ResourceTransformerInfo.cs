#pragma warning disable
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using FactorySimulation.Interfaces;

namespace FactorySimulation;

///<inheritdoc/>
public record ResourceTransformerInfo(string Transformer, (string resourceName, long amount)[] InputResources, (string resourceName, long amount)[] OutputResources, long Time = 1, long Cost = 1) : IResourceTransformerInfo
{
    string serialize((string resourceName, long amount)[] data)
    {
        return JsonSerializer.Serialize(data.Select(x => new string[] { x.resourceName, x.amount.ToString() }),ResourceTransformerInfoExtensions.jsonSerializerOptions())[1..^1];
        // return JsonSerializer.Serialize(data.Select(x=>new string[]{x.resourceName,x.amount.ToString()}))[1..^1];
    }
    ///<inheritdoc/>
    public override string ToString()
    {
        if (InputResources.Zip(OutputResources).All(x => x.First.resourceName == x.Second.resourceName && x.First.amount == x.Second.amount))
            return $"{Transformer}\n{serialize(InputResources)}";
        return $"{Transformer}\n{serialize(InputResources)}\n{serialize(OutputResources)}\nTime: {Time}";
    }
    /// <summary>
    /// Converts this object to json
    /// </summary>
    public string ToJson() => ResourceTransformerInfoExtensions.ToJson(this);
    /// <summary>
    /// Creates resource transformer from json
    /// </summary>
    public static IResourceTransformerInfo FromJson(string json)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json,ResourceTransformerInfoExtensions.jsonSerializerOptions()) ?? throw new ArgumentException("Cannot deserialize json");
        return fromJsonDict(values);
    }
    static IResourceTransformerInfo fromJsonDict(Dictionary<string, JsonElement> values)
    {
        var resourceNameMustBeString = new ArgumentException("Resource name must be string");
        var InputResources = values["InputResources"].EnumerateArray().Select(res => (res[0].GetString() ?? throw resourceNameMustBeString, res[1].GetInt64()));
        var OutputResources = values["OutputResources"].EnumerateArray().Select(res => (res[0].GetString() ?? throw resourceNameMustBeString, res[1].GetInt64()));
        var Time = values["Time"].GetInt64();
        var Cost = values["Cost"].GetInt64();
        var Transformer = values["Transformer"].GetString() ?? throw new ArgumentException("Missing Transformer variable");
        return new ResourceTransformerInfo(Transformer, InputResources.ToArray(), OutputResources.ToArray(), Time, Cost);
    }
    /// <summary>
    /// Converts many resources from single json string.<br/>
    /// </summary>
    /// <param name="json">Contains single array objects that can be converted by <see cref="ResourceTransformerInfo.FromJson(string)"/> </param>
    /// <returns></returns>
    public static IResourceTransformerInfo[] ManyFromJson(string json)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>[]>(json,ResourceTransformerInfoExtensions.jsonSerializerOptions()) ?? throw new ArgumentException("Cannot deserialize json");
        return values.Select(fromJsonDict).ToArray();
    }
};


