namespace FactorySimulation.Interfaces;
/// <summary>
/// Interface that is used to hide mechanism of amount modification
/// </summary>
public interface IModifiableAmount{
    /// <summary>
    /// Method to set amount
    /// </summary>
    void SetAmount(long value);
}
/// <summary>
/// Resource that could only be read
/// </summary>
public interface IReadOnlyResource{
    /// <summary>
    /// Resource name
    /// </summary>
    string Name{get;}
    /// <summary>
    /// Amount of current resources
    /// </summary>
    long Amount{get;}
}
/// <summary>
/// Abstract resource interface
/// </summary>
public interface IResource : IReadOnlyResource{
    /// <summary>
    /// Puts another resource of same type with current resource instance, reducing <paramref name="another"/> resource amount value
    /// </summary>
    /// <param name="another">From where take resources</param>
    /// <param name="amount">How much of another resource to put into current one?</param>
    /// <returns>True if another resource is of same type</returns>
    bool Put(IResource another,long amount = long.MaxValue);
}
