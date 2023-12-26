namespace FactorySimulation;

public class GearFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" gear"] = new ResourceTransformerInfo(
            recipes.CraftingTable,
            new[]{
                (metal+" bolt",4L),
                (metal+" plate",4L),
                (metal+" ring",1L),
            },
            new[]{(metal+" gear",1L)}
        );
    }
}