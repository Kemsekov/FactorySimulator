namespace FactorySimulation.Interfaces;
/// <summary>
/// Information about resource transformation
/// </summary>
public interface IResourceTransformerInfo
{
    /// <summary>
    /// Max possible amount of this transformation happening at the same time. 
    /// So if your recipe requires crafting table, and you may need to scale production using not one, 
    /// but 10 crafting tables, you then can limit how much such "crafting tables" 
    /// can be used at the same time in this particular recipe.
    /// </summary>
    public double MaxAmount {get;}
    /// <summary>
    /// Which resources and in what amount is required to do one transformation?
    /// </summary>
    (string resourceName, long amount)[] InputResources { get; }
    /// <summary>
    /// What is result of one transformation?
    /// </summary>
    (string resourceName, long amount)[] OutputResources { get; }
    /// <summary>
    /// How many simulation ticks one transformation takes?
    /// </summary>
    long Time { get; }
    /// <summary>
    /// What it costs to do one transformation.
    /// </summary>
    long Cost { get; }
    /// <summary>
    /// Name of transformation. Where this transformation happening? Maybe machine name that assembles some detail.
    /// </summary>
    string Transformer{get;}
}

/// <summary>
/// Transforms one resources into another
/// </summary>
public interface IResourceTransformer
{
    /// <summary>
    /// Transformer information about which resources need to be transformed
    /// </summary>
    public IResourceTransformerInfo Info { get; }
    /// <summary>
    /// Input storage for transformer. Here stored all resources that is required for transformation
    /// </summary>
    public IResourceStorage InputStorage { get; }
    /// <summary>
    /// Output storage for transformer. Here stored all transformed resources.
    /// </summary>
    public IResourceStorage OutputStorage { get; }
    /// <summary>
    /// Tries to start resources transformation.<br/>
    /// Takes all necessary resources from it's own input storage, does transformation and puts results into it's own output storage
    /// </summary>
    /// <returns>True if resources is sufficient and transformation is done</returns>
    bool Transform();
}