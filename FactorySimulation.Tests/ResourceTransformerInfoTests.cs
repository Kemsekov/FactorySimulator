namespace FactorySimulation.Tests;

public class ResourceTransformerInfoTests{
    [Fact]
    public void JsonConvert(){
        var res = new ResourceTransformerInfo(
            InputResources:new[]{
                ("A",1l),
                ("B",2l)
            },
            OutputResources:new[]{
                ("C",3l),
                ("D",2l),
            },
            Time: 2l,
            Price: 4
        );
        
        var json = res.ToJson();
        var converted = ResourceTransformerInfo.FromJson(json);
        Assert.Equal(res.InputResources,converted.InputResources);
        Assert.Equal(res.OutputResources,converted.OutputResources);
        Assert.Equal(res.Time,converted.Time);
        Assert.Equal(res.Price,converted.Price);
    }
}
