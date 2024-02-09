#pragma warning disable
using FactorySimulation.Interfaces;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
using ILGPU.Runtime;
using System.Linq;
using System.Reflection;
namespace FactorySimulation;
public static class RecipePipelineBuilder
{
    /// <summary>
    /// Builds required recipe information of how to reproduce it, 
    /// </summary>
    /// <param name="resultGraph">
    /// Resulting resources movement graph.<br/>
    /// Each node represents recipe. <br/>
    /// Each node have properties <br/> 
    /// "recipe" of type <see cref="IResourceTransformerInfo"/> which contain recipe <br/>
    /// "amount" of <see cref="long"/> which contains amount of this recipe reproducers needed <br/>
    /// Each edge have properties <br/>
    /// "resource" of type <see cref="string"/> which contains resource name that is moved trough this edge <br/>
    /// "moved_amount" of type <see cref="string"/> which contains amount of moved resource from one node to another
    /// </param>
    /// <param name="what">What to reproduce</param>
    /// <returns>
    /// Topologically sorted chain of transformations
    /// </returns>
    public static (IResourceTransformerInfo transformer, double amount)[][] BuildRecipe(this IEnumerable<IResourceTransformerInfo> recipes, IResourceTransformerInfo what, double amount,out Graph resultGraph)
    {
        var G = new Graph();
        ResourceTransformerInfo recipe(int nodeId) => G.Nodes[nodeId].Get<ResourceTransformerInfo>("recipe");
        recipes = recipes.Append(what).Distinct().ToList();
        //enumerate recipes and assign indices
        var recipeToId = recipes.ToDictionary(x => x, x => -1);
        int counter = 0;
        foreach (var r in recipes)
        {
            var n = new Node(counter);
            n.Properties["recipe"] = r;
            n.Properties["amount"] = 0.0;
            recipeToId[r] = counter;
            counter++;
            G.Nodes.Add(n);
        }

        //assign root resource amount
        G.Nodes[recipeToId[what]]["amount"] = amount;
        //connect all recipes that is connectable by production line
        //every possible recipe chain is gonna be subgraph of G
        foreach (var n1 in G.Nodes)
            foreach (var n2 in G.Nodes)
            {
                if (n1.Id == n2.Id) continue;
                var info1 = n1.Get<IResourceTransformerInfo>("recipe");
                var info2 = n2.Get<IResourceTransformerInfo>("recipe");

                var canBeMoved = info1.OutputResources
                    .Select(x => x.resourceName)
                    .Intersect(info2.InputResources
                        .Select(x => x.resourceName))
                    .ToArray();
                foreach (var resource in canBeMoved)
                {
                    var edge = new Edge(n2, n1);
                    edge["resource"] = resource;
                    G.Edges.Add(edge);
                }
            }
        // for each low-tier resource in production lines graph
        // create common sink and loop back it to root node.
        // low-tier means some resource that does not have recipe
        // it is simplest raw material
        {
            var sinks = G.Nodes.Where(n => G.Edges.IsSink(n.Id)).ToList();

            var commonSink = new Node(counter);
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
                var edge = new Edge(s, commonSink);
                edge["resource"] = "total";
                G.Edges.Add(edge);
            }

            var connector = new Edge(commonSink.Id, recipeToId[what]);
            connector["resource"] = "total";
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
        //-----------------------
        var whatNode = G.Nodes[recipeToId[what]];
        var resG =  BuildRecipe(G,whatNode,amount);
        resultGraph=G;
        return resG;
        //-----------------------

        var productionLines = new DefaultEdgeSource<Edge>();
        var productionUnits = new DefaultNodeSource<Node>(G.Nodes);

        // repeatedly reconstruct required amount of resources for 
        // each source(end-tier resource) of graph. One by one.
        // And remove reconstructed node for it's full dependencies have been
        // fulfilled
        while (G.Nodes.Count > 0)
        {
            foreach (var n in G.Nodes.ToList())
            {
                // we fulfill requirements only for sources and non-isolated nodes
                if (!G.Edges.IsSource(n.Id) || G.Edges.AdjacentEdges(n.Id).Count() == 0) continue;
                
                var dependencies = G.Edges.OutEdges(n.Id);

                foreach (var e in dependencies)
                {
                    //by one edge only one resource can move and we get that resource here
                    var res = e.Get<string>("resource");
                    var source = G.Nodes[e.TargetId];
                    var target = G.Nodes[e.SourceId];

                    var sourceAmount = source.Get<double>("amount");
                    var targetAmount = target.Get<double>("amount");
                    var sourceRecipe = source.Get<IResourceTransformerInfo>("recipe");
                    var targetRecipe = target.Get<IResourceTransformerInfo>("recipe");
                    //how much of resource `res` we need to put into target in one sec
                    var resourcesNeeded =
                        1.0*targetRecipe.InputResources
                        .First(x => x.resourceName == res)
                        .amount * targetAmount/targetRecipe.Time;
                    
                    //how much of resource `res` one source produce in one sec
                    var produced = 
                        1.0*sourceRecipe.OutputResources
                        .First(x => x.resourceName == res)
                        .amount/sourceRecipe.Time;

                    // find how much of resource `res` is already produced and not consumed yet.
                    // it is equal to:
                    // available = source.Amount*produced - sum( out edges of source with resource `res`)
                    // so can reduce resourcesNeeded=resourcesNeeded-available

                    var available = 
                        source.Get<double>("amount")*produced-
                        productionLines.InEdges(source.Id).Sum(se=>se.Get<double>("moved_amount"));
                    
                    resourcesNeeded-=available;

                    // required amount of sources needed to fulfill resources needs of target
                    // in one sec
                    var requiredAmount = 1.0*resourcesNeeded/produced;

                    source["amount"] = requiredAmount + sourceAmount;

                    // save amount of resource `res` moved by edge in one sec
                    e["moved_amount"] = resourcesNeeded;
                    // and save resulting graph edge
                    productionLines.Add(e);
                    G.Edges.Remove(e);
                }
                G.Nodes.Remove(n);
            }
            G.Do.RemoveIsolatedNodes();
        }

        
        resultGraph = new Graph();
        resultGraph.SetSources(productionUnits,productionLines);
        resultGraph.Do.RemoveIsolatedNodes();

        
        // here we do topological sort
        var clone = new Graph();
        clone.SetSources(productionUnits.AsEnumerable(),productionLines);
        var layers = new List<Node[]>();
        while(clone.Nodes.Count>0){
            var sources = clone.Nodes.Where(n=>clone.Edges.IsSink(n.Id)).ToArray();
            layers.Add(sources);
            clone.Do.RemoveNodes(sources.Select(n=>n.Id).ToArray());
        }

       var result = layers
            .Select(
                x => 
                x.Select(
                    x => 
                    (x.Get<IResourceTransformerInfo>("recipe"), x.Get<double>("amount")))
                    .OrderBy(t=>t.Item2)
                    .ToArray())
            .Reverse()
            .ToArray();

        return result;
    }
    public static (IResourceTransformerInfo transformer, double amount)[][] BuildRecipe(Graph<Node,Edge> G,Node what, double amount,Func<Edge,double>? price = null)
    {
        IResourceTransformerInfo resinfo(Node n)=>n.Get<IResourceTransformerInfo>("recipe");

        price ??= e=>1.0;
        var solver = Solver.CreateSolver("SCIP");

        foreach(var n in G.Nodes){
            var r = resinfo(n);
            n["amount"] = solver.MakeIntVar(0,long.MaxValue,r.ToString());
        }
        foreach(var e in G.Edges){
            var res = e.Get<string>("resource");
            e["moved_amount"] = solver.MakeIntVar(0,long.MaxValue,res);
        }
        var nodes = G.Nodes.ToArray();
        
        var amounts = nodes.Select(n=>n.Get<Variable>("amount")).ToArray();
        var prices = nodes.Select(n=>(double)n.Get<IResourceTransformerInfo>("recipe").Price).ToArray();
        
        var totalPrice = amounts.Dot(prices);

        //set conditions
        foreach(var n in G.Nodes){
            var r = resinfo(n);
            var timeToProcess = r.Time;

            var nAmount = n.Get<Variable>("amount");
            var nAmountInSec = nAmount*1.0/timeToProcess;

            var outE = G.Edges.OutEdges(n.Id);
            var inE = G.Edges.InEdges(n.Id);

            var outResources = 
                inE
                .Select(e=>new{
                    Name=e.Get<string>("resource"),
                    MovedAmount=e.Get<Variable>("moved_amount"),
                    Price=price(e)
                })
                .GroupBy(e=>e.Name)
                .ToDictionary(e=>e.First().Name,e=>e);
            
            var inResources = 
                outE
                .Select(e=>new{
                    Name=e.Get<string>("resource"),
                    MovedAmount=e.Get<Variable>("moved_amount"),
                    Price=price(e)
                })
                .GroupBy(e=>e.Name)
                .ToDictionary(e=>e.First().Name,e=>e);
            
            foreach(var requireResource in r.InputResources){
                var requiredInSec = requireResource.amount*nAmountInSec;
                if(!inResources.ContainsKey(requireResource.resourceName)) continue;

                var inRes = inResources[requireResource.resourceName].ToList();
                if(inRes.Count==0) continue;

                var total = inRes[0].MovedAmount*1.0;
                var totalMovementPrice = inRes[0].MovedAmount*inRes[0].Price;
                foreach(var res in inRes.Skip(1)){
                    total+=res.MovedAmount;
                    totalMovementPrice+=res.MovedAmount*res.Price;
                }

                totalPrice+=totalMovementPrice;
                solver.Add(total>=requiredInSec);
            }

            foreach(var producedResource in r.OutputResources){
                var producedInSec = producedResource.amount*nAmountInSec;
                if(!outResources.ContainsKey(producedResource.resourceName)) continue;

                var outRes = outResources[producedResource.resourceName].ToList();
                if(outRes.Count==0) continue;

                var total = outRes[0].MovedAmount*1.0;
                foreach(var res in outRes.Skip(1))
                    total+=res.MovedAmount;
                
                solver.Add(total<=producedInSec);
            }
        }
        var whatAmount = what.Get<Variable>("amount");
        solver.Add(whatAmount>=amount);

        solver.Minimize(totalPrice);

        var solveRes = solver.Solve();
        var objres = solver.Objective().Value();
        var whatAmountRes = whatAmount.SolutionValue();
        if(solveRes!=Solver.ResultStatus.OPTIMAL) 
            throw new Exception("impossible to build pipeline");

        foreach(var n in G.Nodes){
            n["amount"]=n.Get<Variable>("amount").SolutionValue();
        }
        foreach(var e in G.Edges){
            e["moved_amount"] = e.Get<Variable>("moved_amount").SolutionValue();
        }

        var resultGraph = new Graph();
        resultGraph.SetSources(G.Nodes.Where(n=>n.Get<double>("amount")!=0),G.Edges.Where(e=>e.Get<double>("moved_amount")!=0));
        resultGraph.Do.RemoveIsolatedNodes();
        
        // here we do topological sort
        var clone = new Graph();
        clone.SetSources(resultGraph.Nodes.AsEnumerable(),resultGraph.Edges);
        var layers = new List<Node[]>();
        while(clone.Nodes.Count>0){
            var sources = clone.Nodes.Where(n=>clone.Edges.IsSink(n.Id)).ToArray();
            layers.Add(sources);
            clone.Do.RemoveNodes(sources.Select(n=>n.Id).ToArray());
        }

       var result = layers
            .Select(
                x => 
                x.Select(
                    x => 
                    (x.Get<IResourceTransformerInfo>("recipe"), x.Get<double>("amount")))
                    .OrderBy(t=>t.Item2)
                    .ToArray())
            .Reverse()
            .ToArray();

        return result;
    }
}
