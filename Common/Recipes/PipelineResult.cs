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

/// <summary>
/// Result of pipeline builders
/// </summary>
public class PipelineResult
{
    /// <summary>
    /// Topologically sorted transformations
    /// </summary>
    public (IResourceTransformerInfo transformer, double amount)[][] Transformations { get; init; }
    /// <summary>
    /// Total cost of pipeline
    /// </summary>
    public double TotalCost { get; init; }
    /// <summary>
    /// Resource transformation graph / resources flow graph
    /// </summary>
    public Graph<RecipeNode, ResourceEdge> Graph { get; init; }
}
