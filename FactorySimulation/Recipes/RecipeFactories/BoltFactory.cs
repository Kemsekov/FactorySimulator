namespace FactorySimulation;

public class BoltFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" bolt"] = new ResourceTransformerInfo(
            new[]{
                (metal+" rod",1L),
            },
            new[]{(metal+" bolt",2L)}
        );
    }
}