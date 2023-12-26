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
    public IDictionary<string, IResourceTransformerInfo> Recipe { get; } = new ConcurrentDictionary<string, IResourceTransformerInfo>();
    public IResourceTransformer[] transformers;
    public Recipes(IEnumerable<IMetalPartRecipeFactory> metalPartRecipeFactories)
    {
        //generate metal parts for following metals
        foreach (var m in new[] { "iron", "copper", "gold", "bronze", "steel", "aluminum", "invar", "battery alloy", "titanium", "stainless steel", "tin", "tungsten", "electrum", "platinum" })
        {
            foreach (var factory in metalPartRecipeFactories)
                factory.AddRecipe(m, this);
        }

        Recipe[glass] = new ResourceTransformerInfo(
            RawResource,
            new[]{
                (glass,1L),
            },
            new[] { (glass, 1L) }
        );
        Recipe[glassPane] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                (glass,6L),
            },
            new[] { (glassPane, 16L) }
        );

        Recipe[motor] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("steel rod",2L),
                (magneticSteelRod,1L),
                ("copper wire",4L),
                ("tin cable",2L),
            },
            new[] { (motor, 1L) }
        );
        Recipe[magneticSteelRod] = new ResourceTransformerInfo(
            Polarizer,
            new[]{
                ("steel rod",1L),
            },
            new[] { (magneticSteelRod, 1L) }
        );
        Recipe[redstone] = new ResourceTransformerInfo(
            RawResource,
            new[]{
                (redstone,1L),
            },
            new[] { (redstone, 1L) }
        );

        Recipe[rubber] = new ResourceTransformerInfo(
            RawResource,
            new[]{
                (rubber,1L),
            },
            new[] { (rubber, 1L) }
        );

        Recipe[fluidPipe] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("copper rotor",2L),
                (glassPane,2L),
                ("bronze curved plate",6L)
            },
            new[] { (fluidPipe, 16L) }
        );

        Recipe[pump] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("tin rotor",3L),
                ("tin bolt",2L),
                (motor,1L),
                (fluidPipe,3L),
            },
            new[] { (pump, 1L) }
        );

        Recipe[resistor] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("copper fine wire",2L),
                (coalDust,1L),
                (paper,2L),
            },
            new[] { (resistor, 3L) }
        );

        Recipe[inductor] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("steel rod",1L),
                ("copper wire",8L),
            },
            new[] { (inductor, 1L) }
        );

        Recipe[analogCircuitBoard] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("copper plate",1L),
                (rubber,2L),
            },
            new[] { (analogCircuitBoard, 1L) }
        );

         Recipe[analogCircuit] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                (resistor,2L),
                ("copper wire",3L),
                (analogCircuitBoard,1L),
                (capacitor,2L),
                (inductor,1L)
            },
            new[] { (analogCircuit, 1L) }
        );

        Recipe[piston] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("steel gear",1L),
                (motor,1L),
                ("steel rod",2L),
                ("steel plate",3L),
                ("tin cable",2L),
            },
            new[] { (piston, 1L) }
        );

        Recipe[robotArm] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                (motor,2L),
                (piston,1L),
                ("steel rod",2L),
                ("tin cable",3L),
                (analogCircuit,1L),
            },
            new[] { (robotArm, 1L) }
        );
        Recipe[conveyer] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                (motor,2L),
                (rubber,6L),
                ("tin cable",1L),
            },
            new[] { (conveyer, 1L) }
        );
        Recipe[redstoneBattery] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("battery alloy plate",1L),
                ("battery alloy curved plate",4L),
                ("tin cable",2L),
            },
            new[] { (redstoneBattery, 1L) }
        );

        Recipe[basicMachineHull] = new ResourceTransformerInfo(
            CraftingTable,
            new[]{
                ("steel machine casing",1L),
                (redstoneBattery,2L),
                (analogCircuit,1L),
                ("tin cable",3L),
            },
            new[] { (basicMachineHull, 1L) }
        );

        transformers = Recipe.Values.Select(x => new ResourceTransformer(x)).ToArray();
    }
}
