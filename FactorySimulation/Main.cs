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
        var result = r.BuildRecipe(r.Recipe["steel drill head"], 5);
        foreach (var l in result)
        {
            System.Console.WriteLine("-----------------");
            foreach (var rec in l)
            {
                System.Console.WriteLine(rec.amount);
                System.Console.WriteLine(rec.transformer);
            }
        }
        
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}
