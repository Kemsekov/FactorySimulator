
using System.Text.Json;
using FactorySimulation;
using static AppInstance;

var recip = File.ReadAllText("recipes.json");
var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>[]>(recip);

foreach(var v in values){
    foreach(var res in v["InputResources"].EnumerateArray()){
        var name = res[0].GetString();
        var amount = res[1].GetInt64();
        var a = 1;
    }
}

var builder = CreateHostBuilder(args);
builder.ConfigureServices(Configuration);
App = builder.Build();
App.Run();
