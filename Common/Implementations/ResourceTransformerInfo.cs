#pragma warning disable
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using FactorySimulation.Interfaces;

namespace FactorySimulation;

///<inheritdoc/>
public record ResourceTransformerInfo(string Transformer, (string resourceName, long amount)[] InputResources, (string resourceName, long amount)[] OutputResources, long Time = 1, long Cost = 1,double MaxAmount = double.MaxValue) : IResourceTransformerInfo
{
    string serialize((string resourceName, long amount)[] data)
    {
        return JsonSerializer.Serialize(data.Select(x => new string[] { x.resourceName, x.amount.ToString() }),ResourceTransformerInfoExtensions.jsonSerializerOptions())[1..^1].Replace("\"","");
        // return JsonSerializer.Serialize(data.Select(x=>new string[]{x.resourceName,x.amount.ToString()}))[1..^1];
    }
    ///<inheritdoc/>
    public override string ToString()
    {
        if (InputResources.Zip(OutputResources).All(x => x.First.resourceName == x.Second.resourceName && x.First.amount == x.Second.amount))
            return $"{Transformer}\n{serialize(InputResources)}";
        var timePart = Time!=1 ? $"\nTime: {Time}" : "";
        var costPart = Cost!=1 ? $"\nCost: {Cost}" : "";
        var maxAmountPart = MaxAmount!=double.MaxValue ? $"\nMaxAmount: {MaxAmount}" : "";
        return $"{Transformer}\n{serialize(InputResources)}\n{serialize(OutputResources)}{timePart}{costPart}{maxAmountPart}";
    }
    /// <summary>
    /// Converts this object to json
    /// </summary>
    public string ToJson() => ResourceTransformerInfoExtensions.ToJson(this);
    static IResourceTransformerInfo fromJsonDict(Dictionary<string, JsonElement> values)
    {
        var resourceNameMustBeString = new ArgumentException("Resource name must be string");
        var InputResources = values["InputResources"].EnumerateArray().Select(res => (res[0].GetString() ?? throw resourceNameMustBeString, res[1].GetInt64()));
        var OutputResources = values["OutputResources"].EnumerateArray().Select(res => (res[0].GetString() ?? throw resourceNameMustBeString, res[1].GetInt64()));
        var Time = values["Time"].GetInt64();
        var Cost = values["Cost"].GetInt64();
        var Transformer = values["Transformer"].GetString() ?? throw new ArgumentException("Missing Transformer variable");
        var maxAmount = double.MaxValue;
        if(values.ContainsKey("MaxAmount")){
            maxAmount = values["MaxAmount"].GetDouble();
            if(maxAmount<0)
                maxAmount = double.MaxValue;
        }
        return new ResourceTransformerInfo(Transformer, InputResources.ToArray(), OutputResources.ToArray(), Time, Cost,maxAmount);
    }
    record Input(Dictionary<string, JsonElement>[] Transformations, TransformerLimitation[] TransformersLimitations);
    public record TransformerLimitation(string Transformer, double MaxAmount);
    public record TransformationsConstraints(IResourceTransformerInfo[] Transformations, TransformerLimitation[] TransformerLimitations);
    /// <summary>
    /// Converts many resources from single json string.<br/>
    /// </summary>
    /// <param name="json">Contains single array objects that can be converted by <see cref="ResourceTransformerInfo.FromJson(string)"/> </param>
    /// <returns></returns>
    public static TransformationsConstraints ManyFromJson(string json)
    {
        var values = JsonSerializer.Deserialize<Input>(json,ResourceTransformerInfoExtensions.jsonSerializerOptions()) ?? throw new ArgumentException("Cannot deserialize json");
        var parsedTransformations = values.Transformations.Select(fromJsonDict).ToArray();
        var res = new TransformationsConstraints
            (parsedTransformations,values.TransformersLimitations);
        return res;
    }
};


