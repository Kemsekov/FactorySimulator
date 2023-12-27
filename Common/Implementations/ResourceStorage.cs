using System.Collections.Concurrent;
using FactorySimulation.Interfaces;

namespace FactorySimulation;

/// <summary>
/// Basic resource storage impl
/// </summary>
public class ResourceStorage : IResourceStorage
{
    ///<inheritdoc/>
    public long TotalAmount { get; protected set; } = 0;
    ///<inheritdoc/>
    public long MaxAmountPerResource { get; init; } = long.MaxValue;
    ///<inheritdoc/>
    public long MaxTotalAmount { get; init; } = long.MaxValue;
    ///<inheritdoc/>
    public long ResidualTotalAmount => MaxTotalAmount - TotalAmount;
    ///<inheritdoc/>
    public IEnumerable<IReadOnlyResource> Resources => resources.Values;
    IDictionary<string, IResource> resources;
    /// <param name="storageValues">What resources this storage have</param>
    public ResourceStorage(IEnumerable<IResource>? storageValues = null)
    {
        resources = new ConcurrentDictionary<string, IResource>();
        if (storageValues is not null)
            foreach (var r in storageValues)
                Put(r);
    }
    IResource GetResource(string name)
    {
        if (resources.TryGetValue(name, out var resource) && resource is not null)
        {
            return resource;
        }
        var newRes = Resource.Empty(name);
        resources[name] = newRes;
        return newRes;
    }
    ///<inheritdoc/>
    public bool Put(IResource resource)
    {   
        var stored = GetResource(resource.Name);
        var canPut = new[]{
            resource.Amount,
            ResidualTotalAmount,
            ResidualAmount(resource.Name)
        }.Min();
        if (canPut <= 0) return false;

        stored.Put(resource,canPut);
        TotalAmount += canPut;
        return true;
    }
    ///<inheritdoc/>
    public bool Take(string resourceName, out IResource resource)
    {
        resource = Resource.Empty(resourceName);
        if (resources.TryGetValue(resourceName, out var stored))
        {
            resource.Put(stored,stored.Amount);
            TotalAmount -= resource.Amount;
            return true;
        }
        return false;
    }
    ///<inheritdoc/>
    public bool Take(string resourceName, long amount, out IResource resource)
    {
        resource = Resource.Empty(resourceName);
        if (resources.TryGetValue(resourceName, out var stored))
        {
            resource.Put(stored,amount);
            TotalAmount -= resource.Amount;
            return true;
        }
        return false;
    }
    ///<inheritdoc/>
    public IResource? Take()
    {
        var res = resources.FirstOrDefault().Value;
        if (res is not null)
            TotalAmount -= res.Amount;
        return res;
    }

    ///<inheritdoc/>
    public IReadOnlyResource ReadResource(string resourceName)
    {
        return GetResource(resourceName);
    }
    ///<inheritdoc/>
    public long ResidualAmount(string resourceName) => MaxAmountPerResource - ReadResource(resourceName).Amount;
    ///<inheritdoc/>
    public void Clear()
    {
        resources.Clear();
    }
}
