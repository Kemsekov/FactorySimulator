# Factory simulator
This small application computes optimal way to arrange a set of resource transformations into a dependency / resource flow graph by solving corresponding linear problem.

# Data
Every recipe have following properties:

`Transformer` - name of transformer that uses recipe (cutting machine, compressor, polarizer, etc)
`InputResources` - a list of tuples with required resource name and it's amount
`OutputResources` - a lit of tuples with produced resources and amounts
`Time` - how much time one transformation takes
`Cost` - cost of one resource transformation

And these recipes can be expressed in json format
Example
```json
[{
    "Transformer": "Compressor",
    "InputResources": [
        ["bronze ingot",1]
    ],
    "OutputResources": [
        ["bronze plate",1]
    ],
    "Time": 4,
    "Cost": 3
},
{
    "Transformer": "Cutting machine",
    "InputResources": [
        ["tin ingot",1]
    ],
    "OutputResources": [
        ["tin rod",2]
    ],
    "Time": 1,
    "Cost": 1
},
{
    "Transformer": "Crafting table",
    "InputResources": [
        ["copper rotor",2],
        ["glass pane",2],
        ["bronze curved plate",6]
    ],
    "OutputResources": [
        ["fluid pipe",16]
    ],
    "Time": 10,
    "Cost": 7
}]
```



And these resources can be loaded as follows
```cs
var recipes = ResourceTransformerInfo.ManyFromJson(File.ReadAllText("rep.json"));
```

Now imagine you have hundreds or thousands of such recipes and you need to 
arrange them in such a way, that output resources of some transformations will flow into input of other transformations, so you can build some pipeline that chains multiple resource transformations into big one.

It can be done by converting recipes to graph
```cs
var graph = recipes.ToRecipeGraph();
```
in this graph all recipes will be condensed into nodes, and resource flow edges will be created between these nodes, that corresponds to some resource movement from one transformer to another.

And if you need to limit your graph to just production of some concrete recipe.
```cs
var graph = recipes.ToRecipeGraph(what: resultRecipe,amount: 1);
```

Where `resultRecipe` is some recipe that can be constructed by combining recipes from `recipes` object. So all recipes that is not required will be removed.

# Problem constraints
When you have a resource flow graph, you can impose additional constraints on it, like maximum amount of transformers that implements this recipe
```cs
//now transformation in node 0 can be implemented only two times
graph.Nodes[0].MaxAmount=2;
```

```cs
//now some resource movement in the first edge will be limited to just 5 items per time unit
graph.Edges.First().Capacity=5;
```

# Problem solving
Now when we defined our properties of a graph, all that left to do it decide amounts of transformers for each recipe and amount of flow that goes trough each edge.

So we set our objective to produce `objectiveRecipe` and required amount of it `amountToProduce` and if there is a way to build such flow graph that produces required resource, our `graph` edges and nodes will be updated and we will get valid flow graph that can be used to model real-world production line.

```cs
var result = RecipePipelineBuilder.BuildRecipe(graph,objectiveRecipe,amountToProduce);
```

And this graph's layout can be rendered to use it as a template to plan how to locate corresponding machines.

See `FactorySimulator` example that provides comprehensive example.



