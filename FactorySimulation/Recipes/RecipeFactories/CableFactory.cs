namespace FactorySimulation;

public class CableFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" cable"] = new ResourceTransformerInfo(
            new[]{
                (metal+" wire",3L),
                ("rubber",3L),
            },
            new[]{(metal+" cable",3L)}
        );
    }
}