#pragma warning disable
using System;
using System.Collections.Concurrent;
using FactorySimulation.Interfaces;

namespace FactorySimulation;
/// <summary>
/// Contains minecraft recipes
/// </summary>
public partial class Recipes
{
    public IDictionary<string,IResourceTransformerInfo> Recipe{get;} = new ConcurrentDictionary<string,IResourceTransformerInfo>();
    public IResourceTransformer[] transformers;
    public Recipes(IEnumerable<IMetalPartRecipeFactory> metalPartRecipeFactories){
        foreach(var m in new[]{"iron","copper","gold","bronze","steel","aluminum","invar","battery alloy","titanium","stainless steel","tin","tungsten"}){
            foreach(var factory in metalPartRecipeFactories)
                factory.AddRecipe(m,this);
        }
        Recipe[glass] = new ResourceTransformerInfo(
            new[]{
                (glass,1L),
            },
            new[]{(glass,1L)}
        );
        Recipe[glassPane] = new ResourceTransformerInfo(
            new[]{
                (glass,6L),
            },
            new[]{(glassPane,16L)}
        );
        
        Recipe[motor] = new ResourceTransformerInfo(
            new[]{
                ("steel rod",2L),
                (magneticSteelRod,1L),
                ("copper wire",4L),
                ("tin cable",2L),
            },
            new[]{(motor,1L)}
        );
        Recipe[magneticSteelRod] = new ResourceTransformerInfo(
            new[]{
                ("steel rod",1L),
                (redstone,6L),
            },
            new[]{(magneticSteelRod,1L)}
        );
        Recipe[redstone] = new ResourceTransformerInfo(
            new[]{
                (redstone,1L),
            },
            new[]{(redstone,1L)}
        );
        
        Recipe[rubber] = new ResourceTransformerInfo(
            new[]{
                (rubber,1L),
            },
            new[]{(rubber,1L)}
        );
        
        Recipe[fluidPipe] = new ResourceTransformerInfo(
            new[]{
                ("copper rotor",2L),
                (glassPane,2L),
                ("bronze curved plate",6L)
            },
            new[]{(fluidPipe,16L)}
        );

        transformers = Recipe.Values.Select(x=> new ResourceTransformer(x)).ToArray();
    }
}
