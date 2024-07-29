namespace FactorySimulation.Tests;

public class ResourceTransformerInfoTests
{
    [Fact]
    public void JsonConvertMany()
    {
        var res1 = new ResourceTransformerInfo(
            "Machine1",
            InputResources: new[]{
                ("A1",11L),
                ("B1",22)
            },
            OutputResources: new[]{
                ("C1",33L),
                ("D1",44),
            },
            Time: 12,
            Cost: 13
        );
        var res2 = new ResourceTransformerInfo(
            "Machine2",
            InputResources: new[]{
                ("A2",43L),
                ("B2",314)
            },
            OutputResources: new[]{
                ("C2",32L),
                ("D2",22),
            },
            Time: 22,
            Cost: 42
        );
        var res3 = new ResourceTransformerInfo(
            "Machine3",
            InputResources: new[]{
                ("A3",413L),
                ("B3",314)
            },
            OutputResources: new[]{
                ("C3",232L),
                ("D3",522),
            },
            Time: 122,
            Cost: 242
        );
        var arr = new[] { res1, res2, res3 };
        var json = arr.ManyToJson();
        var converted = ResourceTransformerInfo.ManyFromJson(json);

        foreach (var (p1, p2) in arr.Zip(converted))
        {
            Assert.Equal(p1.Transformer, p2.Transformer);
            Assert.Equal(p1.InputResources, p2.InputResources);
            Assert.Equal(p1.OutputResources, p2.OutputResources);
            Assert.Equal(p1.Time, p2.Time);
            Assert.Equal(p1.Cost, p2.Cost);
        }
    }
}
