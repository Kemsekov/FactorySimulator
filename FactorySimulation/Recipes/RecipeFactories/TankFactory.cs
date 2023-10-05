namespace FactorySimulation;

public class TankFactory : IMetalPartRecipeFactory{
    public void AddRecipe(string metal, Recipes recipes){
        recipes.Recipe[metal+" tank"] = new ResourceTransformerInfo(
            new[]{
                (metal+" plate",8L),
                ("glass",1L),
            },
            new[]{(metal+" tank",1L)}
        );
    }
}
