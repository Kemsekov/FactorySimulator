namespace FactorySimulation;

public class CurvedPlateFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" curved plate"] = new ResourceTransformerInfo(
            new[]{
                (metal+" plate",1L),
            },
            new[]{(metal+" curved plate",1L)}
        );
    }
}
