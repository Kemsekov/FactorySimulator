#pragma warning disable
using FactorySimulation.Interfaces;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
using ILGPU.Runtime;
using System.Linq;
namespace FactorySimulation;
public partial class Recipes
{
    /// <summary>
    /// Builds required recipe information of how to reproduce it, 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="preferredRecipes"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public List<List<(IResourceTransformerInfo transformer, long amount)>> BuildRecipe(IResourceTransformerInfo what, long amount)
    {
        Graph<Node, Edge> G = new Graph();
        ResourceTransformerInfo recipe(int nodeId) => G.Nodes[nodeId].Get<ResourceTransformerInfo>("recipe");
        // true if recipe for given item is already added
        var recipeToId = Recipe.Values.ToDictionary(x => x, x => -1);
        var recipes = Recipe.Values;
        int counter = 0;
        foreach (var r in recipes)
        {
            var n = new Node(counter);
            n.Properties["recipe"] = r;
            n.Properties["amount"] = 0L;
            recipeToId[r] = counter;
            counter++;
            G.Nodes.Add(n);
        }
        G.Nodes[recipeToId[what]].Properties["amount"] = amount;
        foreach (var n1 in G.Nodes)
            foreach (var n2 in G.Nodes)
            {
                if (n1.Id == n2.Id) continue;
                var info1 = n1.Get<IResourceTransformerInfo>("recipe");
                var info2 = n2.Get<IResourceTransformerInfo>("recipe");

                if (info1.OutputResources[0].resourceName == "steel rod" && info2.OutputResources[0].resourceName == "steel ring")
                {
                    System.Console.WriteLine('a');
                }

                var canBeMoved = info1.OutputResources.Select(x => x.resourceName).Intersect(info2.InputResources.Select(x => x.resourceName)).ToArray();
                foreach (var resource in canBeMoved)
                {
                    var edge = new Edge(n2, n1);
                    edge.Properties["resource"] = resource;
                    edge.Properties["executed"] = false;
                    G.Edges.Add(edge);
                }
            }
        var layers = G.Do.TopologicalSort(recipeToId[what]).Layers;
        {
            var sinks = G.Nodes.Where(n => G.Edges.IsSink(n.Id)).ToList();

            var commonSink = new Node(counter);
            commonSink.Properties["recipe"] = new ResourceTransformerInfo(
                "commonSink",
                new[] { ("total", 1L) },
                new[] { ("total", 1L) }
            );
            commonSink.Properties["amount"] = 0L;
            counter++;
            G.Nodes.Add(commonSink);
            foreach (var s in sinks)
            {
                var edge = new Edge(s, commonSink);
                edge.Properties["resource"] = "total";
                edge.Properties["executed"] = true;
                G.Edges.Add(edge);
            }

            var connector = new Edge(commonSink.Id, recipeToId[what]);
            connector.Properties["resource"] = "total";
            connector.Properties["executed"] = true;
            G.Edges.Add(connector);

            var components = G.Do.FindStronglyConnectedComponentsTarjan();
            var allowedNodes = components.Components.First(c => c.nodes.Any(x => x.Id == recipeToId[what])).nodes.Select(x => x.Id).ToArray();
            G.Do.Isolate(commonSink.Id);
            G.SetSources(edges: G.Do.Induce(allowedNodes).Edges);
            G.Do.RemoveIsolatedNodes();
        }
        while (G.Nodes.Count > 0)
        {
            foreach (var n in G.Nodes.ToList())
            {
                if (!G.Edges.IsSource(n.Id) || G.Edges.AdjacentEdges(n.Id).Count() == 0) continue;
                var outE = G.Edges.OutEdges(n.Id);

                foreach (var e in outE)
                {
                    var res = e.Get<string>("resource");
                    var source = G.Nodes[e.TargetId];
                    var target = G.Nodes[e.SourceId];

                    var sourceAmount = source.Get<long>("amount");
                    var targetAmount = target.Get<long>("amount");
                    var sourceRecipe = source.Get<IResourceTransformerInfo>("recipe");
                    var targetRecipe = target.Get<IResourceTransformerInfo>("recipe");
                    var resourcesNeeded = targetRecipe.InputResources.First(x => x.resourceName == res).amount * targetAmount;
                    var produced = sourceRecipe.OutputResources.First(x => x.resourceName == res).amount;
                    var requiredAmount = (long)Math.Ceiling(resourcesNeeded * 1.0 / produced);
                    source.Properties["amount"] = requiredAmount + sourceAmount;
                    G.Edges.Remove(e);
                }
                G.Nodes.Remove(n);
            }
            G.Do.RemoveIsolatedNodes();
        }

        var result = layers.Select(x => x.Select(x => (x.Get<IResourceTransformerInfo>("recipe"), x.Get<long>("amount"))).ToList()).ToList();
        return result;
    }
}
