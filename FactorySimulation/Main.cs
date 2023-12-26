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
        var result = r.BuildRecipe(r.Recipe[r.basicMachineHull], 1);
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
        
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}
