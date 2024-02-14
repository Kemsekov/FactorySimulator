namespace FactorySimulation.Interfaces;

/// <summary>
/// Abstract resource movement interface
/// </summary>
public interface IResourceMovement
{

    /// <summary>
    /// What we are moving
    /// </summary>
    string ResourceName{get;}
    /// <summary>
    /// Amount of resources moving
    /// </summary>
    long Amount{get;}
    /// <summary>
    /// Cost of resource movement
    /// </summary>
    long Cost{get;}
    /// <summary>
    /// Time to move resource from one resource storage to another in simulation ticks.
    /// </summary>
    long Time{get;}
    /// <summary>
    /// From where we are moving resources
    /// </summary>
    IResourceStorage From{get;}
    /// <summary>
    /// To where we are moving resources
    /// </summary>
    IResourceStorage To{get;}
}