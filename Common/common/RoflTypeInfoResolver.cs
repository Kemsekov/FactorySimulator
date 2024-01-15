#pragma warning disable
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// I don't give a f about reflection based serialization safety for dammed Dictionary
/// </summary>
public class RoflTypeInfoResolver : IJsonTypeInfoResolver
{
    //this piece of code is created for a single reason: to serialize json in reflection disabled runtime
    //on strictly known types.
    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        return new DefaultJsonTypeInfoResolver().GetTypeInfo(type,options);
    }
}