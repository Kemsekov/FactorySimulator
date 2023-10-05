using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
public static class IServiceCollectionExtension
{
    /// <summary>
    /// Adds transient services of all types from assembly that inherits from TServiceType.
    /// </summary>
    /// <param name="assembly">Assembly from where to search. Let it be null if you wanna search from TServiceType assembly</param>
    /// <typeparam name="TServiceType">Method will look to assembly from this type </typeparam>
    public static void AddTransientFromAssembly<TServiceType>(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly = assembly ?? typeof(TServiceType).Assembly;
        foreach (var t in assembly.GetTypes())
        {
            if (t.IsClass && t.GetInterfaces().Contains(typeof(TServiceType)))
            {
                services.AddTransient(typeof(TServiceType), t);
                services.AddTransient(t);
            }
        }
    }
    /// <summary>
    /// Adds singleton of all types from assembly that inherits from TServiceType.
    /// </summary>
    /// <param name="assembly">Assembly from where to search. Let it be null if you wanna search from TServiceType assembly</param>
    /// <typeparam name="TServiceType">Method will look to assembly from this type </typeparam>
    public static void AddSingletonFromAssembly<TServiceType>(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly = assembly ?? typeof(TServiceType).Assembly;
        foreach (var t in assembly.GetTypes())
        {
            if (t.IsClass && t.GetInterfaces().Contains(typeof(TServiceType)))
            {
                services.AddSingleton(typeof(TServiceType), t);
                services.AddSingleton(t);
            }
        }
    }
    /// <summary>
    /// Adds scoped of all types from assembly that inherits from TServiceType.
    /// </summary>
    /// <param name="assembly">Assembly from where to search. Let it be null if you wanna search from TServiceType assembly</param>
    /// <typeparam name="TServiceType">Method will look to assembly from this type </typeparam>
    public static void AddScopedFromAssembly<TServiceType>(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly = assembly ?? typeof(TServiceType).Assembly;
        foreach (var t in assembly.GetTypes())
        {
            if (t.IsClass && t.GetInterfaces().Contains(typeof(TServiceType)))
            {
                services.AddScoped(typeof(TServiceType), t);
                services.AddScoped(t);
            }
        }
    }
}