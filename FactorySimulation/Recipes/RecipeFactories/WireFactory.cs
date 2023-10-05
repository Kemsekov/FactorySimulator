namespace FactorySimulation;

public class WireFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" wire"] = new ResourceTransformerInfo(
            new[]{
                (metal+" plate",1L),
            },
            new[]{(metal+" wire",2L)}
        );
    }
}
