using System.Drawing;
using FactorySimulation.Interfaces;
using GraphSharp;
using GraphSharp.GraphDrawer;
using GraphSharp.Graphs;
using ScottPlot;

namespace FactorySimulation;
public class Main : IHostedService
{
    public Recipes Recipes { get; }

    public Main(Recipes recipes)
    {
        Recipes = recipes;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // var r = ResourceTransformerInfo.ManyFromJson(File.ReadAllText("bronze_drill_recipes.json"));
        var r = Recipes.Recipe.Values;
        var nameToRecipe = r.ToDictionary(r => r.OutputResources.First().resourceName, r => r);
        IResourceTransformerInfo recipe = new ResourceTransformerInfo(
            "result",
            [
                ("electric compressor",1),
                ("electric wiremill",1)
            ],
            [("result",1)]
        );
        var G = r.ToRecipeGraph(recipe);
        var recipeNode = G.Nodes.First(n=>n.Recipe==recipe);
        
        var result = RecipePipelineBuilder.BuildRecipe(
            G,
            // (s,cost)=>RecipePipelineBuilder.MaximizeAmountWithLimitedCost(recipeNode,100,s,cost),
            (s,cost)=>RecipePipelineBuilder.ProduceEnoughObjective(recipeNode,1,s,cost),
            RecipePipelineBuilder.IntVar,
            RecipePipelineBuilder.DoubleVar
        );

        PipelineSummary(result.Transformations);
        System.Console.WriteLine("-----------------");
        MachinesSummary(result.Transformations);
        System.Console.WriteLine("-----------------");
        System.Console.WriteLine("Recipies used: " + G.Nodes.Count);
        System.Console.WriteLine("Resource movements: " + G.Edges.Count);
        System.Console.WriteLine("Total cost: " + result.TotalCost);

        Render(G, recipe);
        return Task.CompletedTask;
    }

    void PipelineSummary((IResourceTransformerInfo transformer, double amount)[][] result)
    {
        foreach (var l in result)
        {
            System.Console.WriteLine("-----------------");
            foreach (var rec in l)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("Amount " + rec.amount);
                System.Console.WriteLine(rec.transformer);
            }
        }
    }

    void MachinesSummary((IResourceTransformerInfo transformer, double amount)[][] result)
    {
        var machinesNeeded =
                    result
                    .SelectMany(R => R)
                    .GroupBy(t => t.transformer.Transformer)
                    .Select(c => new { c.First().transformer.Transformer, Amount = c.Count() })
                    .OrderBy(t => t.Amount);
        System.Console.WriteLine("Summary of different recipes machines needed:\n");
        foreach (var m in machinesNeeded)
        {
            System.Console.WriteLine(m.Transformer + " " + m.Amount);
        }
    }
    void Render(Graph<RecipeNode, ResourceEdge> G, IResourceTransformerInfo objective)
    {
        var size = 2000;
        var plt = new Plot(size, size);
        var shapedrawer = new PlotShapeDrawer(size);
        var drawer = new GraphDrawer<RecipeNode, ResourceEdge>(G, shapedrawer, size, n => n.MapProperties().Position);
        drawer.DrawEdges(G.Edges, 0.0006, Color.Blue);
        drawer.DrawNodes(G.Nodes, 0.003, Color.Black);
        foreach (var n in G.Nodes)
        {
            drawer.DrawNodeText(n, Color.DarkGreen, 0.008, n.Recipe.OutputResources.First().resourceName + " " + n.Amount);
        }
        var objNode = G.Nodes.First(n => n.Recipe == objective);
        drawer.DrawNode(objNode, 0.009, Color.Red);

        shapedrawer.plot.SaveFig("image.png");
    }
    /// <summary>
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
