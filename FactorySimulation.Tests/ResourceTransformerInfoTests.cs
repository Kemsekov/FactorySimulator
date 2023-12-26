namespace FactorySimulation.Tests;

public class ResourceTransformerInfoTests
{
    [Fact]
    public void JsonConvert()
    {
        var res = new ResourceTransformerInfo(
            InputResources: new[]{
                ("A",1L),
                ("B",2)
            },
            OutputResources: new[]{
                ("C",3L),
                ("D",2),
            },
            Time: 2,
            Price: 4
        );

        var json = res.ToJson();
        var converted = ResourceTransformerInfo.FromJson(json);
        Assert.Equal(res.InputResources, converted.InputResources);
        Assert.Equal(res.OutputResources, converted.OutputResources);
        Assert.Equal(res.Time, converted.Time);
        Assert.Equal(res.Price, converted.Price);
    }
    [Fact]
    public void JsonConvertMany()
    {
        var res1 = new ResourceTransformerInfo(
            InputResources: new[]{
                ("A1",11L),
                ("B1",22)
            },
            OutputResources: new[]{
                ("C1",33L),
                ("D1",44),
            },
            Time: 12,
            Price: 13
        );
        var res2 = new ResourceTransformerInfo(
            InputResources: new[]{
                ("A2",43L),
                ("B2",314)
            },
            OutputResources: new[]{
                ("C2",32L),
                ("D2",22),
            },
            Time: 22,
            Price: 42
        );
        var res3 = new ResourceTransformerInfo(
            InputResources: new[]{
                ("A3",413L),
                ("B3",314)
            },
            OutputResources: new[]{
                ("C3",232L),
                ("D3",522),
            },
            Time: 122,
            Price: 242
        );
        var arr = new[] { res1, res2, res3 };
        var json = arr.ManyToJson();
        var converted = ResourceTransformerInfo.ManyFromJson(json);

        foreach (var (p1, p2) in arr.Zip(converted))
        {
            Assert.Equal(p1.InputResources, p2.InputResources);
            Assert.Equal(p1.OutputResources, p2.OutputResources);
            Assert.Equal(p1.Time, p2.Time);
            Assert.Equal(p1.Price, p2.Price);
        }
    }
}
