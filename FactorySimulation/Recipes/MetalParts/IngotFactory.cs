namespace FactorySimulation;

public class IngotFactory : IMetalPartRecipeFactory
{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" ingot"] = new ResourceTransformerInfo(
            recipes.RawResource,
            new[]{
                (metal+" ingot",1L),
            },
            new[]{(metal+" ingot",1L)}
        );
    }
}