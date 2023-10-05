using QuikGraph;

namespace FactorySimulation.Tests;

public class ResourcePutTests
{
    [Fact]
    public void Put_SavesTotalAmountOfResources()
    {
        var r1Amount = Random.Shared.Next(100);
        var r2Amount = Random.Shared.Next(100);
        var r1 = new Resource("a",r1Amount);
        var r2 = new Resource("a",r2Amount);

        r1.Put(r2);
        Assert.Equal(r1Amount+r2Amount,r1.Amount);
        Assert.Equal(0L,r2.Amount);
    }
    [Fact]
    public void Put_DoesNotWorkForDifferentResources(){
        var r1Amount = Random.Shared.Next(100);
        var r2Amount = Random.Shared.Next(100);
        var r1 = new Resource("a",r1Amount);
        var r2 = new Resource("b",r2Amount);
        Assert.False(r1.Put(r2));
        Assert.False(r2.Put(r1));

        Assert.Equal(r1Amount,r1.Amount);
        Assert.Equal(r2Amount,r2.Amount);
    }
    [Fact]
    public void Put_DoesNotTakeMoreThanPresent(){
        var r1Amount = Random.Shared.Next(100);
        var r1 = new Resource("a",r1Amount);
        var r2 = Resource.Empty("a");
        r2.Put(r1,1234);
        Assert.Equal(0,r1.Amount);
        Assert.Equal(r1Amount,r2.Amount);
    }
    [Fact]
    public void Take_AbsNegativeValues(){
        var r1Amount = Random.Shared.Next(100)+50;
        var r1 = new Resource("a",r1Amount);
        var r2 = Resource.Empty("a");
        r2.Put(r1,-50);
        Assert.Equal(50,r2.Amount);
        Assert.Equal(r1Amount-r2.Amount,r1.Amount);
    }
    [Fact]
    public void Put_Take_ChaosKeepsTotalAmount(){
        var resources = new List<Resource>();
        var names = new[]{"a","b","c","d"};
        for(int i = 0;i<1000;i++){
            var resName = names[Random.Shared.Next(names.Length)];
            var amount = Random.Shared.Next(20);
            resources.Add(new Resource(resName,amount));
        }
        var before = resources.GroupBy(x=>x.Name).ToDictionary(x=>x.First().Name,x=>x.Sum(x=>x.Amount));
        for(int i = 0;i<10000;i++){
            var exchange = Random.Shared.Next(40)-20;
            var r1 = resources[Random.Shared.Next(resources.Count)];
            var r2 = resources[Random.Shared.Next(resources.Count)];
            if(Random.Shared.Next()%2==0){
                r1.Put(r2,exchange);
            }
            else{
                r2.Put(r1,exchange);
            }
        }
        var after = resources.GroupBy(x=>x.Name).ToDictionary(x=>x.First().Name,x=>x.Sum(x=>x.Amount));
        Assert.Equal(before,after);
    }
}