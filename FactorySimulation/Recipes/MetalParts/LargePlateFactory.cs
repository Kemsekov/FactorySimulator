namespace FactorySimulation;

public class LargePlateFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" large plate"] = new ResourceTransformerInfo(
            recipes.CraftingTable,
            new[]{
                (metal+" plate",4L),
            },
            new[]{(metal+" large plate",1L)}
        );
    }
}
