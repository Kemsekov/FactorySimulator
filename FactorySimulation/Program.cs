
using FactorySimulation;
using static AppInstance;

var builder = CreateHostBuilder(args);
builder.ConfigureServices(Configuration);
App = builder.Build();
App.Run();
