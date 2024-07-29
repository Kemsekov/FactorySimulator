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

public class ResourceEdge : Edge
{
    public ResourceEdge(INode source, INode target) : base(source, target)
    {
    }
    public ResourceEdge(int source, int target) : base(source, target)
    {
    }
    /// <summary>
    /// Cost of moving 1 unit of resource trough this edge
    /// </summary>
    public double Cost{get;set;}=1;
    /// <summary>
    /// Max amount of resources that can be moved in one second
    /// </summary>
    public double Capacity{get;set;}=int.MaxValue;
    /// <summary>
    /// Resource name that is moving by this edge
    /// </summary>
    public string Resource{get;set;}
    /// <summary>
    /// How much resource is moving trough edge in one sec
    /// </summary>
    public double Flow{get;set;}
    /// <summary>
    /// Flow variable that is used by solver
    /// </summary> <summary>
    public Variable FlowVariable{get;set;}
}
