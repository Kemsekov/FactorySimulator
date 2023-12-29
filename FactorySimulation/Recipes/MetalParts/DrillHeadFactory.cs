namespace FactorySimulation;

public class DrillHeadFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" drill head"] = new ResourceTransformerInfo(
            recipes.CraftingTable,
            new[]{
                (metal+" plate",1L),
                (metal+" curved plate",2L),
                (metal+" rod",1L),
                (metal+" gear",2L),
                (recipes.SolderingAlloy,75L),
            },
            new[]{(metal+" drill head",1L)}
        );
    }
}
