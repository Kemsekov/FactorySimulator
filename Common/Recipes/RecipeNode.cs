#pragma warning disable
using FactorySimulation.Interfaces;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Linq;
using System.Reflection;
namespace FactorySimulation;

public class RecipeNode : Node
{
    public RecipeNode(int id) : base(id)
    {
    }
    public IResourceTransformerInfo Recipe{get;set;}
    /// <summary>
    /// Amount of transformers that implements recipe.
    /// </summary>
    public double Amount{get;set;}
    /// <summary>
    /// Max possible amount of this machine
    /// </summary>
    public double MaxAmount{get;set;} = int.MaxValue;
}
