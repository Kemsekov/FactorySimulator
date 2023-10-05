
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FactorySimulation;

/// <summary>
/// Place that contains reference to global application running, so it could be accessed from everywhere
/// </summary>
public static class AppInstance
{
    public static IHost? App { get; set; }
    public static IServiceProvider? Services => App?.Services;
    public static void Configuration(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingletonFromAssembly<IMetalPartRecipeFactory>(typeof(IMetalPartRecipeFactory).Assembly);
        services.AddSingleton<Recipes>();
        services.AddHostedService<Main>();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
            config
            .AddJsonFile(
                "appsettings.json",
                optional: true,
                reloadOnChange: true));
}
