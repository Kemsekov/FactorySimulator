using System.Text.Json;
using System.Text.Json.Serialization;
using FactorySimulation.Interfaces;

namespace FactorySimulation;

///<inheritdoc/>
public record ResourceTransformerInfo((string resourceName, long amount)[] InputResources, (string resourceName, long amount)[] OutputResources, long Time = 1, long Price = 0) : IResourceTransformerInfo{
    string serialize((string resourceName, long amount)[] data){
        return JsonSerializer.Serialize(data.Select(x=>new object[]{x.resourceName,x.amount}))[1..^1];
    }
    ///<inheritdoc/>
    public override string ToString()
    {
        if(InputResources.Zip(OutputResources).All(x=>x.First.resourceName==x.Second.resourceName && x.First.amount==x.Second.amount))
            return $"\n\t{serialize(InputResources)}}}\n";
        return $"\n\t{serialize(InputResources)}\n\t{serialize(OutputResources)}\n";
    }
};


