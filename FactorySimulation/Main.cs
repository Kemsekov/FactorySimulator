using System;

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
        var result = r.BuildRecipe(recipe, 1,out var _);
        foreach (var l in result)
        {
            System.Console.WriteLine("-----------------");
            foreach (var rec in l)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("Amount "+rec.amount);
                System.Console.WriteLine(rec.transformer);
            }
        }

        var raw = 
            result
            .SelectMany(R=>R)
            .Where(t=>t.transformer.TransformationName==recipes.RawResource)
            .OrderBy(t=>t.amount);
        System.Console.WriteLine("Summary of raw resourced needed:\n");
        foreach(var m in raw){
            System.Console.WriteLine(m);
        }

        var machinesNeeded = 
            result
            .SelectMany(R=>R)
            .GroupBy(t=>t.transformer.TransformationName)
            .Select(c=>new{c.First().transformer.TransformationName,Amount=c.Count()})
            .OrderBy(t=>t.Amount);
        System.Console.WriteLine("Summary of machines needed:\n");
        foreach(var m in machinesNeeded){
            System.Console.WriteLine(m.TransformationName+" "+m.Amount);
        }
        System.Console.WriteLine("-----------------");


        
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}
