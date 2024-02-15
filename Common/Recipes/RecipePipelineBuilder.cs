#pragma warning disable
using FactorySimulation.Interfaces;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
using System.Reflection;
namespace FactorySimulation;
public static class RecipePipelineBuilder
{
    /// <summary>
    /// Creates a recipe graph from given recipes.
    /// </summary>
    /// <param name="recipes">A list of recipes</param>
    /// <param name="what">What we need to produce</param>
    /// <param name="amount">Amount of resource that we need to produce</param>
    public static Graph<RecipeNode, ResourceEdge> ToRecipeGraph(this IEnumerable<IResourceTransformerInfo> recipes, IResourceTransformerInfo? what = null)
    {
        var G = new Graph<RecipeNode, ResourceEdge>(i => new(i), (a, b) => new(a, b));
        IResourceTransformerInfo recipe(int nodeId) => G.Nodes[nodeId].Recipe;
        if (what is not null)
            recipes = recipes.Append(what).Distinct().ToList();
        //enumerate recipes and assign indices
        var recipeToId = recipes.ToDictionary(x => x, x => -1);
        int counter = 0;
        foreach (var r in recipes)
        {
            var n = new RecipeNode(counter)
            {
                Recipe = r,
                Amount = 0
            };
            recipeToId[r] = counter;
            counter++;
            G.Nodes.Add(n);
        }

        //connect all recipes that is connectable by production line
        //every possible recipe chain is gonna be subgraph of G
        foreach (var n1 in G.Nodes)
            foreach (var n2 in G.Nodes)
            {
                if (n1.Id == n2.Id) continue;
                var info1 = n1.Recipe;
                var info2 = n2.Recipe;

                var canBeMoved = info1.OutputResources
                    .Select(x => x.resourceName)
                    .Intersect(info2.InputResources
                        .Select(x => x.resourceName))
                    .ToArray();
                foreach (var resource in canBeMoved)
                {
                    var edge = new ResourceEdge(n2, n1)
                    {
                        Resource = resource
                    };
                    G.Edges.Add(edge);
                }
            }
        // for each low-tier resource in production lines graph
        // create common sink and loop back it to root node.
        // low-tier means some resource that does not have recipe
        // it is simplest raw material
        if (what is not null)
        {
            var sinks = G.Nodes.Where(n => G.Edges.IsSink(n.Id)).ToList();

            var commonSink = new RecipeNode(counter);
            commonSink["recipe"] = new ResourceTransformerInfo(
                "commonSink",
                new[] { ("total", 1L) },
                new[] { ("total", 1L) }
            );
            commonSink["amount"] = 0.0;
            counter++;
            G.Nodes.Add(commonSink);
            foreach (var s in sinks)
            {
                var edge = new ResourceEdge(s, commonSink)
                {
                    Resource = "total"
                };
                G.Edges.Add(edge);
            }

            var connector = new ResourceEdge(commonSink.Id, recipeToId[what])
            {
                Resource = "total"
            };
            G.Edges.Add(connector);

            // after we looped back raw resources with root resource
            // we can simply find SCC to get subgraph of all possible ways to 
            // create root resource
            var components = G.Do.FindStronglyConnectedComponentsTarjan();

            // among many components find one that contains our root resource
            var allowedNodes =
                components.Components
                .First(c =>
                    c.nodes
                    .Any(x => x.Id == recipeToId[what]))
                .nodes
                .Select(x => x.Id)
                .ToArray();
            // remove looping back of raw materials
            G.Do.Isolate(commonSink.Id);
            // induce SCC subgraph into G
            G.SetSources(edges: G.Do.Induce(allowedNodes).Edges);
            G.Do.RemoveIsolatedNodes();
        }
        return G;
    }
    /// <summary>
    /// Builds required recipe information of how to reproduce it
    /// </summary>
    /// <param name="what">What to reproduce</param>
    /// <param name="amount">How much to reproduce</param>
    /// <returns>
    /// Pipeline result
    /// </returns>
    public static PipelineResult BuildRecipe(this IEnumerable<IResourceTransformerInfo> recipes, IResourceTransformerInfo what, double amount)
    {
        var G = recipes.ToRecipeGraph(what);
        var whatNode = G.Nodes.First(n => n.Recipe == what);
        var result = BuildRecipe(
            G,
            (s, totalCost) => ProduceEnoughObjective(whatNode, amount, s, totalCost),
            IntVar,
            DoubleVar);
        return result;
    }
    /// <summary>
    /// Builds required recipe information of how to reproduce it
    /// </summary>
    /// <param name="G">Resource flow graph</param>
    /// <param name="what">What to reproduce</param>
    /// <param name="amount">How much to reproduce</param>
    /// <param name="ObjectiveF">
    /// Objective function that takes solver as first param and total cost as second param<br/>
    /// It is good to use one of <br/>
    /// <see cref="ProduceEnoughObjective"/>  <br/>
    /// <see cref="MaximizeAmountWithLimitedCost"/>
    /// </param>
    /// <param name="amountType">
    /// Solver node amount variable type.<br/>
    /// It is good to use one of <br/>
    /// <see cref="IntVar"/>  <br/>
    /// <see cref="DoubleVar"/>
    /// </param>
    /// <param name="flowType">
    /// Solver edge flow variable type.<br/>
    /// It is good to use one of <br/> 
    /// <see cref="IntVar"/> <br/> 
    /// <see cref="DoubleVar"/>
    /// </param>
    /// <returns>
    /// Pipeline result
    /// </returns>
    public static PipelineResult BuildRecipe(Graph<RecipeNode, ResourceEdge> G, Action<Solver, Google.OrTools.LinearSolver.LinearExpr> ObjectiveF, Func<Solver, Variable>? amountType = null, Func<Solver, Variable>? flowType = null)
    {
        amountType ??= s => s.MakeIntVar(0, long.MaxValue, "");
        flowType ??= s => s.MakeIntVar(0, long.MaxValue, "");

        var solver = Solver.CreateSolver("SCIP");

        foreach (var n in G.Nodes)
            n["amount"] = amountType(solver);

        foreach (var e in G.Edges)
            e["flow"] = flowType(solver);

        var nodes = G.Nodes.ToArray();

        var amounts = nodes.Select(n => n.Get<Variable>("amount")).ToArray();
        var costs = nodes.Select(n => (double)n.Recipe.Cost).ToArray();

        var totalCost = amounts.Dot(costs);

        //set conditions
        foreach (var n in G.Nodes)
        {
            var r = n.Recipe;
            var timeToProcess = r.Time;

            var nAmount = n.Get<Variable>("amount");
            var nAmountInSec = nAmount * 1.0 / timeToProcess;

            var outE = G.Edges.OutEdges(n.Id);
            var inE = G.Edges.InEdges(n.Id);

            var outResources =
                inE
                .Select(e => new
                {
                    Name = e.Resource,
                    Flow = e.Get<Variable>("flow"),
                    Cost = e.Cost
                })
                .GroupBy(e => e.Name)
                .ToDictionary(e => e.First().Name, e => e);

            var inResources =
                outE
                .Select(e => new
                {
                    Name = e.Resource,
                    Flow = e.Get<Variable>("flow"),
                    Cost = e.Cost
                })
                .GroupBy(e => e.Name)
                .ToDictionary(e => e.First().Name, e => e);

            foreach (var requireResource in r.InputResources)
            {
                var requiredInSec = requireResource.amount * nAmountInSec;
                if (!inResources.ContainsKey(requireResource.resourceName)) continue;

                var inRes = inResources[requireResource.resourceName].ToList();
                if (inRes.Count == 0) continue;

                var total = inRes[0].Flow * 1.0;
                var totalMovementCost = inRes[0].Flow * inRes[0].Cost;
                foreach (var res in inRes.Skip(1))
                {
                    total += res.Flow;
                    totalMovementCost += res.Flow * res.Cost;
                }

                totalCost += totalMovementCost;
                solver.Add(total >= requiredInSec);
            }

            foreach (var producedResource in r.OutputResources)
            {
                var producedInSec = producedResource.amount * nAmountInSec;
                if (!outResources.ContainsKey(producedResource.resourceName)) continue;

                var outRes = outResources[producedResource.resourceName].ToList();
                if (outRes.Count == 0) continue;

                var total = outRes[0].Flow * 1.0;
                foreach (var res in outRes.Skip(1))
                    total += res.Flow;

                solver.Add(total <= producedInSec);
            }
        }

        foreach (var e in G.Edges)
        {
            var flow = e.Get<Variable>("flow");
            solver.Add(flow <= e.Capacity);
        }

        foreach (var n in G.Nodes)
        {
            var namount = n.Get<Variable>("amount");
            solver.Add(namount <= n.MaxAmount);
        }

        //impose objective goal
        ObjectiveF(solver, totalCost);

        var solveRes = solver.Solve();
        if (solveRes != Solver.ResultStatus.OPTIMAL)
            throw new Exception("impossible to build pipeline");

        foreach (var n in G.Nodes)
        {
            n.Amount = n.Get<Variable>("amount").SolutionValue();
        }
        foreach (var e in G.Edges)
        {
            e.Flow = e.Get<Variable>("flow").SolutionValue();
        }

        var resultGraph = G.CloneJustConfiguration();
        resultGraph.SetSources(G.Nodes.Where(n => n.Amount != 0), G.Edges.Where(e => e.Flow != 0));
        resultGraph.Do.RemoveIsolatedNodes();

        PlanarRender(resultGraph);

        // here we do topological sort
        var result = TopologicalSort(G, resultGraph);

        return new()
        {
            Graph = G,
            TotalCost = G.Nodes.Sum(n => n.Amount * n.Recipe.Cost) + G.Edges.Sum(e => e.Flow * e.Cost),
            Transformations = result
        };
    }

    /// <summary>
    /// Integer variable
    /// </summary>
    public static Variable IntVar(Solver s)
    {
        return s.MakeIntVar(0, long.MaxValue, "");
    }
    /// <summary>
    /// Double variable
    /// </summary>
    public static Variable DoubleVar(Solver s)
    {
        return s.MakeNumVar(0, double.MaxValue, "");
    }

    /// <summary>
    /// This is objective that forces to produce enough amount of required recipe.
    /// </summary>
    public static void ProduceEnoughObjective(Node what, double amount, Solver solver, Google.OrTools.LinearSolver.LinearExpr totalCost)
    {
        var whatAmount = what.Get<Variable>("amount");
        solver.Add(whatAmount >= amount);
        solver.Minimize(totalCost);
    }
    /// <summary>
    /// This is objective that maximizes amount of `what` node recipe production meanwhile limiting the cost
    /// </summary>
    public static void MaximizeAmountWithLimitedCost(Node what, double maxCost, Solver solver, Google.OrTools.LinearSolver.LinearExpr totalCost)
    {
        var whatAmount = what.Get<Variable>("amount");
        solver.Add(totalCost <= maxCost);
        solver.Maximize(whatAmount);
    }
    /// <summary>
    /// </summary>
    static Google.OrTools.Sat.LinearExpr Sum(this IEnumerable<Google.OrTools.Sat.LinearExpr> exp)
    {
        var sum = exp.First() * 1;
        foreach (var e in exp.Skip(1))
            sum += e;
        return sum;
    }
    static void PlanarRender(IGraph<RecipeNode, ResourceEdge> resultGraph)
    {
        // var edges = resultGraph.Edges;
        var pos = resultGraph.Do.Arrange(200, getWeight: e => 1);
        
        foreach (var n in resultGraph.Nodes)
        {
            n.MapProperties().Position = pos[n.Id];
        }
    }
    static (IResourceTransformerInfo Recipe, double Amount)[][] TopologicalSort(Graph<RecipeNode, ResourceEdge> G, IGraph<RecipeNode, ResourceEdge> resultGraph)
    {
        var clone = G.CloneJustConfiguration();
        clone.SetSources(resultGraph.Nodes.AsEnumerable(), resultGraph.Edges);
        var layers = new List<RecipeNode[]>();
        while (clone.Nodes.Count > 0)
        {
            var sources = clone.Nodes.Where(n => clone.Edges.IsSink(n.Id)).ToArray();
            layers.Add(sources);
            clone.Do.RemoveNodes(sources.Select(n => n.Id).ToArray());
        }

        var result = layers
             .Select(
                 x =>
                 x.Select(
                     x =>
                     (x.Recipe, x.Amount))
                     .OrderBy(t => t.Item2)
                     .ToArray())
             .Reverse()
             .ToArray();
        return result;
    }
}
