using System;
using System.Diagnostics;
using System.Drawing;
using FactorySimulation.Interfaces;
using GraphSharp;
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
        var result = r.Recipe.Values.BuildRecipe(recipe, 100, out var G);
        var js = G.Nodes.Select(n=>n.Get<IResourceTransformerInfo>("recipe")).ToArray().ManyToJson();
        File.WriteAllText("rep.json",js);
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
        System.Console.WriteLine("-----------------");

        var sinks = G.Nodes.Where(n=>G.Edges.IsSink(n.Id)).Select(m=>m.Get<IResourceTransformerInfo>("recipe")).ToHashSet();

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
        System.Console.WriteLine("-----------------");
        System.Console.WriteLine("Recipies used: " + G.Nodes.Count);
        System.Console.WriteLine("Resource movements: " + G.Edges.Count);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}
