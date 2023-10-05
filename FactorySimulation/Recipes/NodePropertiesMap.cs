#pragma warning disable
using System;
using GraphSharp.Extensions;

namespace GraphSharp;
public static class NodePropertiesMap1
{
    public static T Get<T>(this INode n, string name)
    {
        object orDefault = n.Properties.GetOrDefault(name);
        if (orDefault is T t)
            return t;
        throw new KeyNotFoundException(name);
    }
    public static T Get<T>(this IEdge n, string name)
    {
        object orDefault = n.Properties.GetOrDefault(name);
        if (orDefault is T t)
            return t;
        throw new KeyNotFoundException(name);
    }
}
