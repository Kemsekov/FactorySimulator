namespace FactorySimulation;

public class PlateFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" plate"] = new ResourceTransformerInfo(
            recipes.Compressor,
            new[]{
                (metal+" ingot",1L),
            },
            new[]{(metal+" plate",1L)}
        );
    }
}
