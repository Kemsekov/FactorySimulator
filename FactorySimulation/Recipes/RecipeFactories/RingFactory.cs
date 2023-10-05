namespace FactorySimulation;

public class RingFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" ring"] = new ResourceTransformerInfo(
            new[]{
                (metal+" rod",1L),
            },
            new[]{(metal+" ring",1L)}
        );
    }
}
