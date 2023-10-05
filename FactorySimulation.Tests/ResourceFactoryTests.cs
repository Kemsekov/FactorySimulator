namespace FactorySimulation.Tests;
public class ResourceFactoryTests
{
    [Fact]
    public void Put(){
        var r1 = new Resource("A",3);
        var r2 = new Resource("A",3);
        var r3 = new Resource("B",10);
        var storage = new ResourceStorage(){MaxTotalAmount=10,MaxAmountPerResource=5};

        Assert.True(storage.Put(r1));
        Assert.Equal(3,storage.TotalAmount);
        Assert.Equal(0,r1.Amount);

        Assert.True(storage.Put(r2));
        Assert.Equal(5,storage.TotalAmount);
        Assert.Equal(1,r2.Amount);

        Assert.True(storage.Put(r3));
        Assert.Equal(10,storage.TotalAmount);
        Assert.Equal(5,r3.Amount);

        Assert.False(storage.Put(r3));
        Assert.Equal(10,storage.TotalAmount);
        Assert.Equal(5,r3.Amount);
    }
    [Fact]
    public void Take(){
        var r1 = new Resource("A",3);
        var r2 = new Resource("A",4);
        var r3 = new Resource("B",10);
        var storage = new ResourceStorage(){MaxTotalAmount=15,MaxAmountPerResource=6};
        storage.Put(r1);
        storage.Put(r2);
        storage.Put(r3);

        Assert.True(storage.Take("B",out var r));
        Assert.Equal(6,r.Amount);
        Assert.Equal(4,r3.Amount);
        Assert.Equal(0,r1.Amount);
        Assert.Equal(1,r2.Amount);
    }
}
