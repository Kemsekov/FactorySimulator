using System;

namespace FactorySimulation;
/// <summary>
/// Creates all recipes for rotor
/// </summary>
public class RotorFactory : IMetalPartRecipeFactory
{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" rotor"] = new ResourceTransformerInfo(
            recipes.CraftingTable,
            new[]{
                (metal+" blade",4L),
                (metal+" bolt",4L),
                (metal+" ring",1L),
            },
            new[]{(metal+" rotor",1L)}
        );
    }
}
