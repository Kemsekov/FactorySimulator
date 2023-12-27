using FactorySimulation.Interfaces;

namespace FactorySimulation;

/// <summary>
/// Basic resource impl.<br/> 
/// Good rule: this class should be created only with one purpose - to initialize resources <br/>
/// All other operations should be performed by <see cref="Put"/>
/// </summary>
public class Resource : IResource, IModifiableAmount
{
    /// <summary>
    /// Creates new empty resource
    /// </summary>
    public static IResource Empty(string name) => new Resource(name,0);
    /// <summary>
    /// Creates new resource
    /// </summary>
    public Resource(string name, long amount)
    {
        Name = name;
        Amount = Math.Abs(amount);
    }
    ///<inheritdoc/>
    public string Name { get; init; }
    ///<inheritdoc/>
    public long Amount { get; private set; }
    ///<inheritdoc/>
    public bool Put(IResource another,long amount = long.MaxValue)
    {
        if(another.Name!=Name) return false;
        var canMove = Math.Min(another.Amount,Math.Abs(amount));
        if(another is IModifiableAmount r){
            r.SetAmount(another.Amount-canMove);
            Amount+=canMove;
            return true;
        }
        return false;
    }
    /// <inheritdoc/>
    public override string ToString()
    {
        return $"({Name} : {Amount})";
    }

    void IModifiableAmount.SetAmount(long value)
    {
        Amount = value;
    }
}
