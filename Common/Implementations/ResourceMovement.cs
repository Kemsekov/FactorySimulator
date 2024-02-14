using System;
using FactorySimulation.Interfaces;
using QuikGraph;

namespace FactorySimulation;
/// <summary>
/// Resource movement record
/// </summary>
public record ResourceMovement : IResourceMovement
{
    /// <summary>
    /// Pipes groups of transformers.<br/>
    /// Each previous group is considered input to next group
    /// </summary>
    /// <param name="transformers">Transformations groups</param>
    /// <param name="amount">How much each type of resources each resource movement can move at the time</param>
    /// <param name="time">How long each movement take</param>
    /// <param name="cost">Cost of each movement</param>
    public static IEnumerable<IResourceMovement> Pipe(IEnumerable<IEnumerable<IResourceTransformer>> transformers, long amount, long time = 1, long cost = 0)
    {
        amount = Math.Abs(amount);
        var result = new List<IResourceMovement>();
        transformers.Aggregate((t1Gropup, t2Group) =>
        {
            foreach(var t1 in t1Gropup)
            foreach(var t2 in t2Group)
            {
                var canBeMoved = t1.Info.OutputResources.Select(x => x.resourceName).Intersect(t2.Info.InputResources.Select(x => x.resourceName)).ToArray();
                foreach (var resource in canBeMoved)
                {
                    var movement = new ResourceMovement()
                    {
                        From = t1.OutputStorage,
                        To = t2.InputStorage,
                        Time = time,
                        Amount = amount,
                        Cost = cost,
                        ResourceName = resource
                    };
                    result.Add(movement);
                }
            }
            return t2Group;
        });
        return result;
    }
    /// <summary>
    /// Pipes several transformations into bunch of resource movements in a straight manner. <br/>
    /// It takes two neighbor transformations and tries to pipe intersection of their resources <br/>
    /// Neighbor transformations - those that are next to each other in the order you have provided as input<br/>
    /// For example if (transformation 1) produces resources (A,B,C) and (transformation 2) takes as input resources (A,C) this pipe
    /// will create two resource movements for them, that takes resource A from output of (transformation 1) and puts it into input of (transformation 2),
    /// same for resource B.
    /// </summary>
    /// <param name="transformers">Transformations</param>
    /// <param name="amount">How much each type of resources each resource movement can move at the time</param>
    /// <param name="time">How long each movement take</param>
    /// <param name="cost">Cost of each movement</param>
    public static IEnumerable<IResourceMovement> PipeStraight(IEnumerable<IResourceTransformer> transformers, long amount, long time = 1, long cost = 0)
    {
        amount = Math.Abs(amount);
        var result = new List<IResourceMovement>();
        transformers.Aggregate((t1, t2) =>
        {
            var canBeMoved = t1.Info.OutputResources.Select(x => x.resourceName).Intersect(t2.Info.InputResources.Select(x => x.resourceName)).ToArray();
            foreach (var resource in canBeMoved)
            {
                var movement = new ResourceMovement()
                {
                    From = t1.OutputStorage,
                    To = t2.InputStorage,
                    Time = time,
                    Amount = amount,
                    Cost = cost,
                    ResourceName = resource
                };
                result.Add(movement);
            }
            return t2;
        });
        return result;
    }
    /// <summary>
    /// Pipes several transformations into bunch of resource movements in by connecting everything that is connectable. <br/>
    /// It takes all pairs of two transformations and tries to pipe intersection of their resources <br/>
    /// For example if (transformation 1) produces resources (A,B,C) and (transformation 2) takes as input resources (A,C) this pipe
    /// will create two resource movements for them, that takes resource A from output of (transformation 1) and puts it into input of (transformation 2),
    /// same for resource B. <br/>
    /// In contrast to <see cref="PipeStraight"/>, this method consider all possible pairs of transformations, building every possible connection between them,
    /// which can lead (WARNING!!) to cycles in movements.
    /// </summary>
    /// <param name="transformers">Transformations</param>
    /// <param name="amount">How much each type of resources each resource movement can move at the time</param>
    /// <param name="time">How long each movement take</param>
    /// <param name="cost">Cost of each movement</param>
    public static IEnumerable<IResourceMovement> PipeEverything(IEnumerable<IResourceTransformer> transformers, long amount, long time = 1, long cost = 0)
    {
        amount = Math.Abs(amount);
        var result = new List<IResourceMovement>();
        foreach (var t1 in transformers)
            foreach (var t2 in transformers)
            {
                if(t1.Equals(t2)) continue;
                var canBeMoved = t1.Info.OutputResources.Select(x => x.resourceName).Intersect(t2.Info.InputResources.Select(x => x.resourceName)).ToArray();
                foreach (var resource in canBeMoved)
                {
                    var movement = new ResourceMovement()
                    {
                        From = t1.OutputStorage,
                        To = t2.InputStorage,
                        Time = time,
                        Amount = amount,
                        Cost = cost,
                        ResourceName = resource
                    };
                    result.Add(movement);
                }
            }
        return result;
    }

    ///<inheritdoc/>
    public required string ResourceName { get; init; }

    ///<inheritdoc/>
    public long Amount { get; init; }

    ///<inheritdoc/>
    public long Cost { get; init; }

    ///<inheritdoc/>
    public long Time { get; init; }

    ///<inheritdoc/>
    public required IResourceStorage From { get; init; }

    ///<inheritdoc/>
    public required IResourceStorage To { get; init; }
}
