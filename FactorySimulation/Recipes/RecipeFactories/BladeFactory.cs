namespace FactorySimulation;

public class BladeFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" blade"] = new ResourceTransformerInfo(
            new[]{
                (metal+" curved plate",2L),
                (metal+" rod",1L),
            },
            new[]{(metal+" blade",4L)}
        );
    }
}
