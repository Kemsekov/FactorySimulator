namespace FactorySimulation;

public class GearFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" gear"] = new ResourceTransformerInfo(
            recipes.CraftingTable,
            new[]{
                (metal+" plate",4L),
                (metal+" ring",1L),
                (recipes.SolderingAlloy,100L),
            },
            new[]{(metal+" gear",2L)}
        );
    }
}