using System;
using System.Diagnostics;
using System.Drawing;
using FactorySimulation.Interfaces;
using GraphSharp;
using GraphSharp.GraphDrawer;
using GraphSharp.Graphs;
using MathNet.Numerics.LinearAlgebra.Single;
using Microsoft.AspNetCore.Components.Forms;
using ScottPlot;

namespace FactorySimulation;
public class Main : IHostedService
{
    private Recipes recipes;

    public Main(Recipes recipes)
    {
        this.recipes = recipes;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var r = recipes;
        var recipe = r.Recipe[r.basicMachineHull];
        recipe = r.Recipe["bronze drill head"];
        recipe = new ResourceTransformerInfo(
            Transformer: "Result",
            InputResources: new[]{
                (recipes.ElectricCompressor,1L),
                (recipes.ElectricCuttingMachine,2L),
                (recipes.ElectricMixer,1L),
                (recipes.ElectricWiremill,3L),
            },
            OutputResources: new[]{
                ("result",1L)
            });
        // var js = G.Nodes.Select(n => n.Recipe).ToArray().ManyToJson();
        // File.WriteAllText("rep.json", js);

        var result = r.Recipe.Values.BuildRecipe(recipe, 100, out var G);

        PipelineSummary(result);
        System.Console.WriteLine("-----------------");
        MachinesSummary(result);

        System.Console.WriteLine("-----------------");
        System.Console.WriteLine("Recipies used: " + G.Nodes.Count);
        System.Console.WriteLine("Resource movements: " + G.Edges.Count);
        Render(G);
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
        System.Console.WriteLine("Summary of machines needed:\n");
        foreach (var m in machinesNeeded)
        {
            System.Console.WriteLine(m.Transformer + " " + m.Amount);
        }
    }
    void Render(Graph<RecipeNode, ResourceEdge> G){
        var size = 2000;
        var plt = new Plot(size,size);
        var shapedrawer = new PlotShapeDrawer(size);
        var drawer = new GraphDrawer<RecipeNode,ResourceEdge>(G,shapedrawer,size,n=>n.MapProperties().Position);
        drawer.DrawEdges(G.Edges,0.0006,Color.Blue);
        drawer.DrawNodes(G.Nodes,0.005,Color.Red);
        drawer.DrawNodeIds(G.Nodes,Color.DarkBlue,0.005);
        shapedrawer.plot.SaveFig("image.png");
    }
    /// <summary>
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
