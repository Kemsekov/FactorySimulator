namespace FactorySimulation;

public class MachineCashingFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" machine casing"] = new ResourceTransformerInfo(
            new[]{
                (metal+" plate",8L),
                (metal+" gear",1L),
            },
            new[]{(metal+" machine casing",1L)}
        );
    }
}