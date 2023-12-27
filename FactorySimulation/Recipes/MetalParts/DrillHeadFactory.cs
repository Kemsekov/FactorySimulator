namespace FactorySimulation;

public class DrillHeadFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" drill head"] = new ResourceTransformerInfo(
            recipes.CraftingTable,
            new[]{
                (metal+" plate",1L),
                (metal+" curved plate",2L),
                (metal+" rod",1L),
                (metal+" bolt",3L),
                (metal+" gear",2L),
            },
            new[]{(metal+" drill head",4L)}
        );
    }
}
