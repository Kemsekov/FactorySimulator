namespace FactorySimulation.Interfaces;
/// <summary>
/// Resource storage, that can hold resources of different types
/// </summary>
public interface IResourceStorage{
/// <summary>
    /// Total amount of resources stored
    /// </summary>
    long TotalAmount{get;}
    /// <summary>
    /// Maximum amount per resource that can be stored
    /// </summary>
    long MaxAmountPerResource{get;}
    /// <summary>
    /// Maximum total amount of resources that can be stored
    /// </summary>
    long MaxTotalAmount{get;}
    /// <summary>
    /// How much total space left
    /// </summary>
    long ResidualTotalAmount => MaxTotalAmount-TotalAmount;
    /// <summary>
    /// How much space per resource is left
    /// </summary>
    long ResidualAmount(string resourceName);
    /// <summary>
    /// Collection of resources stored in current storage.
    /// </summary>
    IEnumerable<IReadOnlyResource> Resources{get;}
    /// <returns>Read only resource. This method can be used to check if resource is present into storage, or how many of some resource is present</returns>
    IReadOnlyResource ReadResource(string resourceName);
    /// <summary>
    /// Puts resource into storage.
    /// </summary>
    /// <returns>True if successfully putted resource, else false</returns>
    bool Put(IResource resource);
    /// <summary>
    /// Tries to take resource with given name from storage.<br/>
    /// Takes all of resources of given name.
    /// </summary>
    /// <returns>True if resource is present into storage and successfully taken, else False</returns>
    bool Take(string resourceName, out IResource resource);
    /// <summary>
    /// Tries to take some amount of resource with given name from storage.<br/>
    /// If <paramref name="amount"/> is greater than amount of resources stored, it will return
    /// all stored resources instead, so you can get smaller amount of resources than requested.
    /// </summary>
    /// <returns>True if resource is present into storage and successfully taken, else False</returns>
    bool Take(string resourceName, long amount, out IResource resource);
    /// <summary>
    /// Takes first found resource from storage
    /// </summary>
    IResource? Take();
    /// <summary>
    /// Removed everything from storage
    /// </summary>
    public void Clear();
}
