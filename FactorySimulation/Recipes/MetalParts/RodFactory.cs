namespace FactorySimulation;

public class RodFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" rod"] = new ResourceTransformerInfo(
            recipes.CuttingMachine,
            new[]{
                (metal+" ingot",1L),
            },
            new[]{(metal+" rod",2L)}
        );
    }
}
