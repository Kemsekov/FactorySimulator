namespace FactorySimulation.Tests;

public class ToRecipeGraphTests
{
    [Fact]
    public void ToRecipeGraph_Works(){
        var recipes = ResourceTransformerInfo.ManyFromJson(File.ReadAllText("rep.json"));
        var nameToRecipe = recipes.ToDictionary(r=>r.OutputResources.First().resourceName,r=>r);
        var graph = recipes.ToRecipeGraph();
        var graphRecipes = graph.Nodes.Select(n=>n.Recipe).OrderBy(r=>r.GetHashCode());
        Assert.Equal(recipes.OrderBy(r=>r.GetHashCode()),graphRecipes);

        foreach(var e in graph.Edges){
            var source = graph.Nodes[e.SourceId].Recipe;
            var target = graph.Nodes[e.TargetId].Recipe;
            var edgeRes = 
                target.OutputResources
                .Select(r=>r.resourceName)
                .Intersect(
                    source.InputResources
                    .Select(r=>r.resourceName)
                ).First();
            Assert.Equal(edgeRes,e.Resource);
        }

    }
}
