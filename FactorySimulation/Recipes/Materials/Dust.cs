namespace FactorySimulation;


public interface IDustable{
    void AddRecipe(string material, Recipes recipes);
}

public class Dust : IDustable{
    public void AddRecipe(string material, Recipes recipes){
        recipes.Recipe[material+" dust"] = new ResourceTransformerInfo(
            recipes.Macerator,
            new[]{
                (material,1L),
            },
            new[]{(material+" dust",1L)}
        );
    }
}

public class TinyDust : IDustable{
    public void AddRecipe(string material, Recipes recipes){
        recipes.Recipe[material+" tiny dust"] = new ResourceTransformerInfo(
            recipes.Macerator,
            new[]{
                (material,1L),
            },
            new[]{(material+" tiny dust",1L)}
        );
    }
}
