using System.Transactions;
using FactorySimulation.Interfaces;

namespace FactorySimulation;
/// <summary>
/// Contains factory simulation implementation
/// </summary>
public class Simulation
{
    /// <summary>
    /// How long in simulation ticks given object is occupied and cannot work
    /// </summary>
    IDictionary<object, long> occupiedTicks;
    /// <summary>
    /// How many time given object is doing nothing
    /// </summary>
    IDictionary<object, long> waitingTimeTicks;
    /// <summary>
    /// How much money did we spent so far for operating object
    /// </summary>
    IDictionary<object, long> totalOperationCost;
    IResourceMovement[] movements;
    IResourceTransformer[] transformations;
    /// <summary>
    /// How many simulation steps is complete
    /// </summary>
    public long Steps{get;private set;}
    /// <summary>
    /// Creates new instance of factory simulation 
    /// </summary>
    /// <param name="resourceMovements">How our resources should move?</param>
    /// <param name="resourceTransformations">What transformations to resources we do?</param>
    public Simulation(IEnumerable<IResourceMovement> resourceMovements, IEnumerable<IResourceTransformer> resourceTransformations)
    {
        this.occupiedTicks = new Dictionary<object, long>();
        this.waitingTimeTicks = new Dictionary<object, long>();
        this.totalOperationCost = new Dictionary<object, long>();
        foreach (var r in resourceMovements)
        {
            waitingTimeTicks[r] = 0;
            occupiedTicks[r] = 0;
            totalOperationCost[r] = 0;
        }
        foreach (var t in resourceTransformations)
        {
            waitingTimeTicks[t] = 0;
            occupiedTicks[t] = 0;
            totalOperationCost[t] = 0;
        }
        movements = resourceMovements.ToArray();
        transformations = resourceTransformations.ToArray();
    }
    /// <summary>
    /// Step in simulation
    /// </summary>
    public void Step()
    {
        for(int i = 0;i<movements.Length;i++)
        {
            var movement = movements[(i+Steps) % movements.Length];
            var ticks = occupiedTicks[movement];
            if (ticks > 0)
            {
                occupiedTicks[movement] = ticks - 1;
                continue;
            }
            var canMove = new[]{
                movement.To.ResidualAmount(movement.ResourceName), //how much we can put
                movement.Amount, //how much we want to move
                movement.From.ReadResource(movement.ResourceName).Amount} // how much we can take
                .Min();

            if (canMove > 0)
            {
                movement.From.Take(movement.ResourceName, canMove, out var res);
                movement.To.Put(res ?? Resource.Empty(movement.ResourceName));
                occupiedTicks[movement] = Math.Abs(movement.Time);
                totalOperationCost[movement] += movement.Cost;
            }
            else
                waitingTimeTicks[movement] += 1;
        }
        for(int i = 0;i<transformations.Length;i++)
        {
            var transformation = transformations[(i+Steps) % transformations.Length];
            var ticks = occupiedTicks[transformation];
            if (ticks > 0)
            {
                occupiedTicks[transformation] = ticks - 1;
                continue;
            }
            if (transformation.Transform())
            {
                occupiedTicks[transformation] = Math.Abs(transformation.Info.Time);
                totalOperationCost[transformation] += transformation.Info.Cost;
            }
            else
                waitingTimeTicks[transformation] += 1;
        }
        Steps++;
    }
    long GetValueOrZero(IDictionary<object, long> dict, object key)
    {
        if (dict.TryGetValue(key, out var res))
        {
            return res;
        }
        return 0;
    }
    /// <returns>Value between 0 and 1 indicating progress of movement.</returns>
    public double Progress(IResourceMovement m){
        var waitingTime = GetValueOrZero(occupiedTicks,m);
        return waitingTime*1.0/m.Time;
    }
    /// <returns>Value between 0 and 1 indicating progress of resource transformer.</returns>
    public double Progress(IResourceTransformer m){
        var waitingTime = GetValueOrZero(occupiedTicks,m);
        return waitingTime*1.0/m.Info.Time;
    }
    /// <summary>
    /// How much time movement is not moving anything because <see cref="IResourceMovement.From"/>  is empty or <see cref="IResourceMovement.To"/> cannot take any items anymore
    /// </summary>
    /// <returns>Time this movement did nothing, expressed in simulation ticks</returns>
    public long WaitingTimes(IResourceMovement m)
    {
        return GetValueOrZero(waitingTimeTicks, m);
    }
    /// <summary>
    /// How much time transformer is not doing anything because <see cref="IResourceTransformer.InputStorage"/> does not have enough resources, or <see cref="IResourceTransformer.OutputStorage"/> cannot hold any more resources
    /// </summary>
    /// <returns>Time this movement did nothing, expressed in simulation ticks</returns>
    public long WaitingTimes(IResourceTransformer t)
    {
        return GetValueOrZero(waitingTimeTicks, t);
    }
    /// <summary>
    /// How much money given movement operation consumed so far
    /// </summary>
    public long TotalCost(IResourceMovement m)
    {
        return GetValueOrZero(totalOperationCost, m);
    }
    /// <summary>
    /// How much money given transformer operation consumed so far
    /// </summary>
    public long TotalCost(IResourceTransformer t)
    {
        return GetValueOrZero(totalOperationCost, t);
    }
    /// <summary>
    /// Get view on total amount of resources in simulation
    /// </summary>
    public IReadOnlyResource[] TotalResources
    {
        get
        {
            var totalResources = new Dictionary<string, long>();
            foreach (var storage in transformations.Select(x => x.InputStorage.Resources.Concat(x.OutputStorage.Resources)))
            {
                foreach (var m in storage)
                {
                    if (!totalResources.ContainsKey(m.Name))
                    {
                        totalResources[m.Name] = m.Amount;
                        continue;
                    }
                    totalResources[m.Name] += m.Amount;
                }
            }
            return totalResources.Select(x => new Resource(x.Key, x.Value)).ToArray();
        }
    }
}