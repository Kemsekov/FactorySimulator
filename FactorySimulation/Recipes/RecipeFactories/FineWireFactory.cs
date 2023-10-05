namespace FactorySimulation;

public class FineWireFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" fine wire"] = new ResourceTransformerInfo(
            new[]{
                (metal+" wire",1L),
            },
            new[]{(metal+" fine wire",4L)}
        );
    }
}