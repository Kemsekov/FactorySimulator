using System;
using FactorySimulation.Interfaces;

namespace FactorySimulation;

/// <summary>
/// Delegate-based resource transformer impl
/// </summary>
public class ResourceTransformer : IResourceTransformer
{
    ///<inheritdoc/>
    public IResourceStorage InputStorage{get;init;}
     ///<inheritdoc/>
    public IResourceStorage OutputStorage{get;init;}
    /// <summary>
    /// Transformer information about which resources need to be transformed
    /// </summary>
    public IResourceTransformerInfo Info { get; }
    /// <summary>
    /// Total amount of transformed resources
    /// </summary>
    public long TotalAmount => OutputStorage.TotalAmount;
    ///<inheritdoc/>
    public IEnumerable<IReadOnlyResource> Resources => OutputStorage.Resources;

    /// <summary>
    /// Creates new resource transformer
    /// </summary>
    /// <param name="info">What transformation we do</param>
    /// <param name="inputStorage">transformer's input storage</param>
    /// <param name="outputStorage">transformer's output storage</param>
    public ResourceTransformer(IResourceTransformerInfo info, IResourceStorage? inputStorage = null, IResourceStorage? outputStorage = null){
        InputStorage = inputStorage ?? new ResourceStorage();
        OutputStorage = outputStorage ?? new ResourceStorage();
        Info = info;
    }

    bool FullfilRequirements(){
        foreach(var requirement in Info.OutputResources){
            if(OutputStorage.ResidualAmount(requirement.resourceName)<requirement.amount)
                return false;
        }
        foreach(var requirement in Info.InputResources){
            if(InputStorage.ReadResource(requirement.resourceName).Amount<requirement.amount)
                return false;
        }
        return true;
    }
    ///<inheritdoc/>
    public bool Transform()
    {
        if(!FullfilRequirements()) return false;
        foreach(var requirement in Info.InputResources){
            InputStorage.Take(requirement.resourceName,requirement.amount,out var resource);       
        }
        foreach(var requirement in Info.OutputResources){
            var result = new Resource(requirement.resourceName,requirement.amount);
            OutputStorage.Put(result);
        }
        return true;
    }
}

