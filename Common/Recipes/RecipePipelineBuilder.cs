#pragma warning disable
using FactorySimulation.Interfaces;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
using ILGPU.Runtime;
using System.Linq;
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
    public static List<List<(IResourceTransformerInfo transformer, double amount)>> BuildRecipe(this IDictionary<string, IResourceTransformerInfo> recipes_, IResourceTransformerInfo what, double amount,out Graph resultGraph)
    {
        Graph<Node, Edge> G = new Graph();
        ResourceTransformerInfo recipe(int nodeId) => G.Nodes[nodeId].Get<ResourceTransformerInfo>("recipe");
        //enumerate recipes and assign indices
        var recipeToId = recipes_.Values.ToDictionary(x => x, x => -1);
        var recipes = recipes_.Values;
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
        //add resource that we need to build if it is not present among recipes
        if(!recipeToId.ContainsKey(what)){
            var n = new Node(counter);
            n.Properties["recipe"] = what;
            n.Properties["amount"] = 0.0;
            recipeToId[what] = counter;
            counter++;
            G.Nodes.Add(n);
        }

        //assign root resource amount
        G.Nodes[recipeToId[what]].Properties["amount"] = amount;
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
                    edge.Properties["resource"] = resource;
                    G.Edges.Add(edge);
                }
            }
        // Do topological sort starting from root node
        var layers = G.Do.TopologicalSort(recipeToId[what]).Layers;
        // for each low-tier resource in production lines graph
        // create common sink and loop back it to root node.
        // low-tier means some resource that does not have recipe
        // it is simplest raw material
        {
            var sinks = G.Nodes.Where(n => G.Edges.IsSink(n.Id)).ToList();

            var commonSink = new Node(counter);
            commonSink.Properties["recipe"] = new ResourceTransformerInfo(
                "commonSink",
                new[] { ("total", 1L) },
                new[] { ("total", 1L) }
            );
            commonSink.Properties["amount"] = 0.0;
            counter++;
            G.Nodes.Add(commonSink);
            foreach (var s in sinks)
            {
                var edge = new Edge(s, commonSink);
                edge.Properties["resource"] = "total";
                G.Edges.Add(edge);
            }

            var connector = new Edge(commonSink.Id, recipeToId[what]);
            connector.Properties["resource"] = "total";
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

        var productionLines = new DefaultEdgeSource<Edge>();
        var productionUnits = new DefaultNodeSource<Node>();

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

                    source.Properties["amount"] = requiredAmount + sourceAmount;

                    // save amount of resource `res` moved by edge in one sec
                    e.Properties["moved_amount"] = resourcesNeeded;
                    // and save resulting graph edge
                    productionLines.Add(e);
                    G.Edges.Remove(e);

                }
                G.Nodes.Remove(n);

                // save resulting graph nodes
                productionUnits.Add(n);
            }
            G.Do.RemoveIsolatedNodes();
        }

        var result = layers
            .Select(
                x => 
                x.Select(
                    x => 
                    (x.Get<IResourceTransformerInfo>("recipe"), x.Get<double>("amount")))
                    .OrderBy(t=>t.Item2)
                    .ToList())
            .ToList();
        resultGraph = new Graph();
        resultGraph.SetSources(productionUnits,productionLines);
        return result;
    }
}
